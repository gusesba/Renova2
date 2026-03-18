using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.CommercialRules;
using Renova.Services.Features.Credits;
using Renova.Services.Features.SupplierPayments.Abstractions;
using Renova.Services.Features.SupplierPayments.Contracts;

namespace Renova.Services.Features.SupplierPayments.Services;

// Implementa o modulo 11 com obrigacoes, liquidacoes e comprovante textual.
public sealed class SupplierPaymentService : ISupplierPaymentService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public SupplierPaymentService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega filtros e listas auxiliares do modulo na loja ativa.
    /// </summary>
    public async Task<SupplierPaymentWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureSupplierPaymentViewContextAsync(cancellationToken);
        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var suppliers = await (
                from pessoaLoja in _dbContext.PessoaLojas.AsNoTracking()
                join pessoa in _dbContext.Pessoas.AsNoTracking() on pessoaLoja.PessoaId equals pessoa.Id
                where pessoaLoja.LojaId == context.LojaId
                where pessoaLoja.EhFornecedor
                orderby pessoa.Nome
                select new SupplierPaymentSupplierOptionResponse(
                    pessoa.Id,
                    pessoa.Nome,
                    pessoa.Documento))
            .ToListAsync(cancellationToken);

        var paymentMethods = await _dbContext.MeiosPagamento
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId && x.Ativo)
            .OrderBy(x => x.Nome)
            .Select(x => new SupplierPaymentMethodOptionResponse(
                x.Id,
                x.Nome,
                x.TipoMeioPagamento,
                SupplierPaymentValues.GetPaymentMethodTypeLabel(x.TipoMeioPagamento)))
            .ToListAsync(cancellationToken);

        return new SupplierPaymentWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            paymentMethods,
            suppliers,
            SupplierPaymentValues.BuildObligationStatusOptions()
                .Select(x => new SupplierPaymentOptionResponse(x.Codigo, x.Nome))
                .ToArray(),
            SupplierPaymentValues.BuildObligationTypeOptions()
                .Select(x => new SupplierPaymentOptionResponse(x.Codigo, x.Nome))
                .ToArray(),
            SupplierPaymentValues.BuildLiquidationTypeOptions()
                .Select(x => new SupplierPaymentOptionResponse(x.Codigo, x.Nome))
                .ToArray());
    }

    /// <summary>
    /// Lista obrigacoes da loja ativa com filtros por fornecedor, tipo e status.
    /// </summary>
    public async Task<IReadOnlyList<SupplierObligationSummaryResponse>> ListarAsync(
        SupplierPaymentListQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureSupplierPaymentViewContextAsync(cancellationToken);

        var obligationsQuery =
            from obrigacao in _dbContext.ObrigacoesFornecedor.AsNoTracking()
            join fornecedor in _dbContext.Pessoas.AsNoTracking() on obrigacao.PessoaId equals fornecedor.Id
            join peca in _dbContext.Pecas.AsNoTracking() on obrigacao.PecaId equals peca.Id into pieceGroup
            from peca in pieceGroup.DefaultIfEmpty()
            join produto in _dbContext.ProdutoNomes.AsNoTracking() on peca.ProdutoNomeId equals produto.Id into productGroup
            from produto in productGroup.DefaultIfEmpty()
            join item in _dbContext.VendaItens.AsNoTracking() on obrigacao.VendaItemId equals item.Id into itemGroup
            from item in itemGroup.DefaultIfEmpty()
            join venda in _dbContext.Vendas.AsNoTracking() on item.VendaId equals venda.Id into saleGroup
            from venda in saleGroup.DefaultIfEmpty()
            where obrigacao.LojaId == context.LojaId
            select new
            {
                Obrigacao = obrigacao,
                FornecedorNome = fornecedor.Nome,
                FornecedorDocumento = fornecedor.Documento,
                CodigoInternoPeca = peca != null ? peca.CodigoInterno : null,
                ProdutoNomePeca = produto != null ? produto.Nome : null,
                VendaId = venda != null ? venda.Id : (Guid?)null,
                NumeroVenda = venda != null ? venda.NumeroVenda : null,
            };

        if (query.PessoaId.HasValue)
        {
            obligationsQuery = obligationsQuery.Where(x => x.Obrigacao.PessoaId == query.PessoaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.TipoObrigacao))
        {
            var type = SupplierPaymentValues.NormalizeObligationType(query.TipoObrigacao);
            obligationsQuery = obligationsQuery.Where(x => x.Obrigacao.TipoObrigacao == type);
        }

        if (!string.IsNullOrWhiteSpace(query.StatusObrigacao))
        {
            var status = SupplierPaymentValues.NormalizeStoredObligationStatus(query.StatusObrigacao);
            obligationsQuery = obligationsQuery.Where(x =>
                status == SupplierPaymentValues.ObligationStatuses.Pendente
                    ? x.Obrigacao.StatusObrigacao == "aberta" ||
                      x.Obrigacao.StatusObrigacao == SupplierPaymentValues.ObligationStatuses.Pendente
                    : x.Obrigacao.StatusObrigacao == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            obligationsQuery = obligationsQuery.Where(x =>
                x.FornecedorNome.ToLower().Contains(term) ||
                x.FornecedorDocumento.ToLower().Contains(term) ||
                (x.CodigoInternoPeca ?? string.Empty).ToLower().Contains(term) ||
                (x.ProdutoNomePeca ?? string.Empty).ToLower().Contains(term) ||
                (x.NumeroVenda ?? string.Empty).ToLower().Contains(term));
        }

        var obligations = await obligationsQuery
            .OrderByDescending(x => x.Obrigacao.DataGeracao)
            .ThenByDescending(x => x.Obrigacao.CriadoEm)
            .ToListAsync(cancellationToken);

        if (obligations.Count == 0)
        {
            return [];
        }

        var obligationIds = obligations.Select(x => x.Obrigacao.Id).ToArray();
        var liquidationStats = await _dbContext.LiquidacoesObrigacaoFornecedor
            .AsNoTracking()
            .Where(x => obligationIds.Contains(x.ObrigacaoFornecedorId))
            .GroupBy(x => x.ObrigacaoFornecedorId)
            .Select(group => new
            {
                ObrigacaoId = group.Key,
                Total = group.Sum(x => x.Valor),
                Count = group.Count(),
            })
            .ToDictionaryAsync(x => x.ObrigacaoId, cancellationToken);

        return obligations
            .Select(item =>
            {
                var stats = liquidationStats.GetValueOrDefault(item.Obrigacao.Id);
                var valorLiquidado = stats?.Total ?? 0m;

                return new SupplierObligationSummaryResponse(
                    item.Obrigacao.Id,
                    item.Obrigacao.PessoaId,
                    item.FornecedorNome,
                    item.FornecedorDocumento,
                    item.Obrigacao.PecaId,
                    item.CodigoInternoPeca,
                    item.ProdutoNomePeca,
                    item.VendaId,
                    item.NumeroVenda,
                    item.Obrigacao.TipoObrigacao,
                    SupplierPaymentValues.NormalizeStoredObligationStatus(item.Obrigacao.StatusObrigacao),
                    item.Obrigacao.ValorOriginal,
                    item.Obrigacao.ValorEmAberto,
                    valorLiquidado,
                    stats?.Count ?? 0,
                    item.Obrigacao.DataGeracao,
                    item.Obrigacao.DataVencimento,
                    item.Obrigacao.Observacoes)
                ;
            })
            .ToArray();
    }

    /// <summary>
    /// Carrega o detalhe completo da obrigacao com historico de liquidacoes.
    /// </summary>
    public async Task<SupplierObligationDetailResponse> ObterDetalheAsync(
        Guid obrigacaoId,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureSupplierPaymentViewContextAsync(cancellationToken);
        var summary = await LoadObligationSummaryAsync(context.LojaId, obrigacaoId, cancellationToken)
            ?? throw new InvalidOperationException("Obrigacao nao encontrada na loja ativa.");

        var liquidations = await (
                from liquidacao in _dbContext.LiquidacoesObrigacaoFornecedor.AsNoTracking()
                join usuario in _dbContext.Usuarios.AsNoTracking() on liquidacao.LiquidadoPorUsuarioId equals usuario.Id
                join meio in _dbContext.MeiosPagamento.AsNoTracking() on liquidacao.MeioPagamentoId equals meio.Id into paymentMethodGroup
                from meio in paymentMethodGroup.DefaultIfEmpty()
                where liquidacao.ObrigacaoFornecedorId == obrigacaoId
                orderby liquidacao.LiquidadoEm descending
                select new SupplierPaymentLiquidationResponse(
                    liquidacao.Id,
                    liquidacao.TipoLiquidacao,
                    liquidacao.MeioPagamentoId,
                    meio != null ? meio.Nome : null,
                    liquidacao.ContaCreditoLojaId,
                    liquidacao.Valor,
                    liquidacao.ComprovanteUrl,
                    liquidacao.LiquidadoEm,
                    liquidacao.LiquidadoPorUsuarioId,
                    usuario.Nome,
                    liquidacao.Observacoes))
            .ToListAsync(cancellationToken);

        return new SupplierObligationDetailResponse(
            summary,
            liquidations,
            BuildReceiptText(summary, liquidations));
    }

    /// <summary>
    /// Liquida total ou parcialmente a obrigacao com pagamento financeiro, credito ou ambos.
    /// </summary>
    public async Task<SupplierObligationDetailResponse> LiquidarAsync(
        Guid obrigacaoId,
        SettleSupplierObligationRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureSupplierPaymentManageContextAsync(cancellationToken);
        var normalizedPayments = NormalizeSettlementPayments(request.Pagamentos);

        var obligation = await _dbContext.ObrigacoesFornecedor
            .FirstOrDefaultAsync(x => x.Id == obrigacaoId && x.LojaId == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Obrigacao nao encontrada na loja ativa.");

        var currentStatus = SupplierPaymentValues.NormalizeStoredObligationStatus(obligation.StatusObrigacao);
        if (currentStatus == SupplierPaymentValues.ObligationStatuses.Paga ||
            currentStatus == SupplierPaymentValues.ObligationStatuses.Cancelada)
        {
            throw new InvalidOperationException("A obrigacao informada nao aceita novas liquidacoes.");
        }

        var totalSettlement = RoundMoney(normalizedPayments.Sum(x => x.Valor));
        if (totalSettlement > obligation.ValorEmAberto)
        {
            throw new InvalidOperationException("O valor informado excede o saldo em aberto da obrigacao.");
        }

        var paymentMethodIds = normalizedPayments
            .Where(x => x.TipoLiquidacao == SupplierPaymentValues.LiquidationTypes.MeioPagamento)
            .Select(x => x.MeioPagamentoId)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();

        var paymentMethods = await _dbContext.MeiosPagamento
            .Where(x => x.LojaId == context.LojaId && x.Ativo)
            .Where(x => paymentMethodIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (paymentMethods.Count != paymentMethodIds.Length)
        {
            throw new InvalidOperationException("Informe apenas meios de pagamento ativos da loja.");
        }

        ContaCreditoLoja? creditAccount = null;
        if (normalizedPayments.Any(x => x.TipoLiquidacao == SupplierPaymentValues.LiquidationTypes.CreditoLoja))
        {
            creditAccount = await EnsureSupplierCreditAccountAsync(
                context.LojaId,
                obligation.PessoaId,
                context.UsuarioId,
                cancellationToken);
        }

        var before = SnapshotObligation(obligation);
        var now = DateTimeOffset.UtcNow;
        var note = NormalizeOptionalText(request.Observacoes) ?? "Liquidacao registrada pelo modulo de pagamentos.";
        var receiptUrl = NormalizeOptionalText(request.ComprovanteUrl);
        var liquidations = new List<LiquidacaoObrigacaoFornecedor>();
        var financialMovements = new List<MovimentacaoFinanceira>();
        var creditMovements = new List<MovimentacaoCreditoLoja>();

        foreach (var payment in normalizedPayments)
        {
            var liquidation = new LiquidacaoObrigacaoFornecedor
            {
                Id = Guid.NewGuid(),
                ObrigacaoFornecedorId = obligation.Id,
                TipoLiquidacao = payment.TipoLiquidacao,
                MeioPagamentoId = payment.TipoLiquidacao == SupplierPaymentValues.LiquidationTypes.MeioPagamento
                    ? payment.MeioPagamentoId
                    : null,
                ContaCreditoLojaId = payment.TipoLiquidacao == SupplierPaymentValues.LiquidationTypes.CreditoLoja
                    ? creditAccount?.Id
                    : null,
                Valor = payment.Valor,
                ComprovanteUrl = receiptUrl,
                LiquidadoEm = now,
                LiquidadoPorUsuarioId = context.UsuarioId,
                Observacoes = note,
                CriadoPorUsuarioId = context.UsuarioId,
            };

            liquidations.Add(liquidation);

            if (payment.TipoLiquidacao == SupplierPaymentValues.LiquidationTypes.MeioPagamento)
            {
                financialMovements.Add(new MovimentacaoFinanceira
                {
                    Id = Guid.NewGuid(),
                    LojaId = context.LojaId,
                    TipoMovimentacao = SupplierPaymentValues.FinancialMovementTypes.PagamentoFornecedor,
                    Direcao = "saida",
                    MeioPagamentoId = payment.MeioPagamentoId,
                    LiquidacaoObrigacaoFornecedorId = liquidation.Id,
                    ValorBruto = payment.Valor,
                    Taxa = 0m,
                    ValorLiquido = payment.Valor,
                    Descricao = $"Pagamento ao fornecedor da obrigacao {obligation.Id}.",
                    CompetenciaEm = now,
                    MovimentadoEm = now,
                    MovimentadoPorUsuarioId = context.UsuarioId,
                    CriadoPorUsuarioId = context.UsuarioId,
                });
            }
            else
            {
                if (creditAccount is null)
                {
                    throw new InvalidOperationException("Conta de credito nao encontrada para o fornecedor.");
                }

                var previousBalance = creditAccount.SaldoAtual;
                creditAccount.SaldoAtual = RoundMoney(creditAccount.SaldoAtual + payment.Valor);
                TouchEntity(creditAccount, context.UsuarioId);

                creditMovements.Add(new MovimentacaoCreditoLoja
                {
                    Id = Guid.NewGuid(),
                    ContaCreditoLojaId = creditAccount.Id,
                    TipoMovimentacao = CreditValues.MovementTypes.CreditoRepasse,
                    OrigemTipo = CreditValues.Origins.RepasseFornecedor,
                    OrigemId = obligation.Id,
                    Valor = payment.Valor,
                    SaldoAnterior = previousBalance,
                    SaldoPosterior = creditAccount.SaldoAtual,
                    Observacoes = note,
                    MovimentadoEm = now,
                    MovimentadoPorUsuarioId = context.UsuarioId,
                    CriadoPorUsuarioId = context.UsuarioId,
                });
            }
        }

        obligation.ValorEmAberto = RoundMoney(obligation.ValorEmAberto - totalSettlement);
        obligation.StatusObrigacao = obligation.ValorEmAberto == 0m
            ? SupplierPaymentValues.ObligationStatuses.Paga
            : SupplierPaymentValues.ObligationStatuses.Parcial;
        TouchEntity(obligation, context.UsuarioId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.LiquidacoesObrigacaoFornecedor.AddRange(liquidations);
        _dbContext.MovimentacoesFinanceiras.AddRange(financialMovements);
        _dbContext.MovimentacoesCreditoLoja.AddRange(creditMovements);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "obrigacao_fornecedor",
            obligation.Id,
            "liquidada",
            before,
            SnapshotObligation(obligation),
            cancellationToken);

        foreach (var liquidation in liquidations)
        {
            await _auditService.RegistrarAuditoriaAsync(
                context.LojaId,
                "liquidacao_obrigacao_fornecedor",
                liquidation.Id,
                "criada",
                null,
                SnapshotLiquidation(liquidation),
                cancellationToken);
        }

        foreach (var movement in financialMovements)
        {
            await _auditService.RegistrarAuditoriaAsync(
                context.LojaId,
                "movimentacao_financeira",
                movement.Id,
                "criada",
                null,
                SnapshotFinancialMovement(movement),
                cancellationToken);
        }

        foreach (var movement in creditMovements)
        {
            await _auditService.RegistrarAuditoriaAsync(
                context.LojaId,
                "movimentacao_credito_loja",
                movement.Id,
                "criada",
                null,
                SnapshotCreditMovement(movement),
                cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return await ObterDetalheAsync(obligation.Id, cancellationToken);
    }

    /// <summary>
    /// Carrega um resumo unico da obrigacao para a tela de detalhe.
    /// </summary>
    private async Task<SupplierObligationSummaryResponse?> LoadObligationSummaryAsync(
        Guid lojaId,
        Guid obrigacaoId,
        CancellationToken cancellationToken)
    {
        var items = await ListarAsync(
            new SupplierPaymentListQueryRequest(null, null, null, null),
            cancellationToken);

        return items.FirstOrDefault(x => x.Id == obrigacaoId && x.PessoaId != Guid.Empty);
    }

    /// <summary>
    /// Normaliza e valida as linhas de liquidacao do payload.
    /// </summary>
    private static IReadOnlyList<NormalizedSettlementPayment> NormalizeSettlementPayments(
        IReadOnlyList<SettleSupplierObligationPaymentItemRequest> payments)
    {
        if (payments.Count == 0)
        {
            throw new InvalidOperationException("Informe ao menos uma forma de liquidacao.");
        }

        var normalized = payments.Select(payment =>
        {
            var type = SupplierPaymentValues.NormalizeLiquidationType(payment.TipoLiquidacao);
            var value = RoundMoney(payment.Valor);
            if (value <= 0m)
            {
                throw new InvalidOperationException("Informe apenas valores positivos na liquidacao.");
            }

            if (type == SupplierPaymentValues.LiquidationTypes.MeioPagamento && !payment.MeioPagamentoId.HasValue)
            {
                throw new InvalidOperationException("Selecione o meio de pagamento para a linha financeira.");
            }

            return new NormalizedSettlementPayment(type, payment.MeioPagamentoId, value);
        }).ToArray();

        return normalized;
    }

    /// <summary>
    /// Garante uma conta de credito ativa para repasses ao fornecedor.
    /// </summary>
    private async Task<ContaCreditoLoja> EnsureSupplierCreditAccountAsync(
        Guid lojaId,
        Guid pessoaId,
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var account = await _dbContext.ContasCreditoLoja
            .FirstOrDefaultAsync(x => x.LojaId == lojaId && x.PessoaId == pessoaId, cancellationToken);

        if (account is null)
        {
            account = new ContaCreditoLoja
            {
                Id = Guid.NewGuid(),
                LojaId = lojaId,
                PessoaId = pessoaId,
                SaldoAtual = 0m,
                SaldoComprometido = 0m,
                StatusConta = CreditValues.AccountStatuses.Ativa,
                CriadoPorUsuarioId = usuarioId,
            };

            _dbContext.ContasCreditoLoja.Add(account);
        }

        if (!CreditValues.CanReceiveCredits(account.StatusConta))
        {
            throw new InvalidOperationException("A conta de credito do fornecedor esta bloqueada.");
        }

        return account;
    }

    /// <summary>
    /// Exige autenticao, loja ativa e permissao de consulta do modulo.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureSupplierPaymentViewContextAsync(CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para continuar.");

        await EnsureStoreMembershipAsync(usuarioId, lojaId, cancellationToken);

        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.FinanceiroVisualizar, AccessPermissionCodes.FinanceiroConciliar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui acesso ao modulo de pagamentos e repasses.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Exige permissao explicita de conciliacao financeira para liquidar obrigacoes.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureSupplierPaymentManageContextAsync(CancellationToken cancellationToken)
    {
        var context = await EnsureSupplierPaymentViewContextAsync(cancellationToken);
        var hasPermission = await HasPermissionAsync(
            context.UsuarioId,
            context.LojaId,
            [AccessPermissionCodes.FinanceiroConciliar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui permissao para liquidar repasses a fornecedor.");
        }

        return context;
    }

    /// <summary>
    /// Garante que o usuario continua vinculado a loja ativa.
    /// </summary>
    private async Task EnsureStoreMembershipAsync(Guid usuarioId, Guid lojaId, CancellationToken cancellationToken)
    {
        var hasMembership = await _dbContext.UsuarioLojas.AnyAsync(
            x => x.UsuarioId == usuarioId &&
                 x.LojaId == lojaId &&
                 x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                 (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
            cancellationToken);

        if (!hasMembership)
        {
            throw new InvalidOperationException("Voce nao possui acesso a loja ativa informada.");
        }
    }

    /// <summary>
    /// Verifica se o usuario possui alguma permissao na matriz de cargos da loja.
    /// </summary>
    private async Task<bool> HasPermissionAsync(
        Guid usuarioId,
        Guid lojaId,
        IReadOnlyCollection<string> permissionCodes,
        CancellationToken cancellationToken)
    {
        return await (
                from usuarioLoja in _dbContext.UsuarioLojas
                join usuarioLojaCargo in _dbContext.UsuarioLojaCargos on usuarioLoja.Id equals usuarioLojaCargo.UsuarioLojaId
                join cargo in _dbContext.Cargos on usuarioLojaCargo.CargoId equals cargo.Id
                join cargoPermissao in _dbContext.CargoPermissoes on cargo.Id equals cargoPermissao.CargoId
                join permissao in _dbContext.Permissoes on cargoPermissao.PermissaoId equals permissao.Id
                where usuarioLoja.UsuarioId == usuarioId
                where usuarioLoja.LojaId == lojaId
                where usuarioLoja.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo
                where usuarioLoja.DataFim == null || usuarioLoja.DataFim >= DateTimeOffset.UtcNow
                where cargo.Ativo && permissao.Ativo
                where permissionCodes.Contains(permissao.Codigo)
                select permissao.Id)
            .AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Gera o comprovante textual unico da liquidacao da obrigacao.
    /// </summary>
    private static string BuildReceiptText(
        SupplierObligationSummaryResponse obligation,
        IReadOnlyList<SupplierPaymentLiquidationResponse> liquidations)
    {
        var lines = new List<string>
        {
            "COMPROVANTE DE PAGAMENTO AO FORNECEDOR",
            string.Empty,
            $"Fornecedor: {obligation.FornecedorNome}",
            $"Documento: {obligation.FornecedorDocumento}",
            $"Obrigacao: {obligation.Id}",
            $"Tipo: {obligation.TipoObrigacao}",
            $"Status: {obligation.StatusObrigacao}",
            $"Valor original: {obligation.ValorOriginal:F2}",
            $"Valor em aberto: {obligation.ValorEmAberto:F2}",
        };

        if (!string.IsNullOrWhiteSpace(obligation.CodigoInternoPeca))
        {
            lines.Add($"Peca: {obligation.CodigoInternoPeca} - {obligation.ProdutoNomePeca}");
        }

        if (!string.IsNullOrWhiteSpace(obligation.NumeroVenda))
        {
            lines.Add($"Venda relacionada: {obligation.NumeroVenda}");
        }

        lines.Add(string.Empty);
        lines.Add("Liquidacoes:");

        if (liquidations.Count == 0)
        {
            lines.Add("- Nenhuma liquidacao registrada.");
        }
        else
        {
            foreach (var liquidation in liquidations.OrderBy(x => x.LiquidadoEm))
            {
                var origin = liquidation.TipoLiquidacao == SupplierPaymentValues.LiquidationTypes.MeioPagamento
                    ? liquidation.MeioPagamentoNome ?? "Meio financeiro"
                    : "Credito da loja";

                lines.Add(
                    $"- {liquidation.LiquidadoEm:dd/MM/yyyy HH:mm} | {origin} | {liquidation.Valor:F2} | {liquidation.LiquidadoPorUsuarioNome}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Atualiza os campos de auditoria da entidade.
    /// </summary>
    private static void TouchEntity(AuditEntityBase entity, Guid usuarioId)
    {
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Arredonda valores monetarios para duas casas.
    /// </summary>
    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Normaliza um texto opcional para armazenamento consistente.
    /// </summary>
    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    /// <summary>
    /// Gera snapshot sintetico da obrigacao para auditoria.
    /// </summary>
    private static object SnapshotObligation(ObrigacaoFornecedor obligation)
    {
        return new
        {
            obligation.Id,
            obligation.LojaId,
            obligation.PessoaId,
            obligation.PecaId,
            obligation.VendaItemId,
            obligation.TipoObrigacao,
            obligation.StatusObrigacao,
            obligation.ValorOriginal,
            obligation.ValorEmAberto,
            obligation.DataGeracao,
            obligation.DataVencimento,
            obligation.Observacoes,
        };
    }

    /// <summary>
    /// Gera snapshot sintetico da liquidacao para auditoria.
    /// </summary>
    private static object SnapshotLiquidation(LiquidacaoObrigacaoFornecedor liquidation)
    {
        return new
        {
            liquidation.Id,
            liquidation.ObrigacaoFornecedorId,
            liquidation.TipoLiquidacao,
            liquidation.MeioPagamentoId,
            liquidation.ContaCreditoLojaId,
            liquidation.Valor,
            liquidation.ComprovanteUrl,
            liquidation.LiquidadoEm,
            liquidation.LiquidadoPorUsuarioId,
            liquidation.Observacoes,
        };
    }

    /// <summary>
    /// Gera snapshot sintetico do movimento financeiro.
    /// </summary>
    private static object SnapshotFinancialMovement(MovimentacaoFinanceira movement)
    {
        return new
        {
            movement.Id,
            movement.LojaId,
            movement.TipoMovimentacao,
            movement.Direcao,
            movement.MeioPagamentoId,
            movement.LiquidacaoObrigacaoFornecedorId,
            movement.ValorBruto,
            movement.Taxa,
            movement.ValorLiquido,
            movement.Descricao,
            movement.MovimentadoEm,
            movement.MovimentadoPorUsuarioId,
        };
    }

    /// <summary>
    /// Gera snapshot sintetico do movimento de credito.
    /// </summary>
    private static object SnapshotCreditMovement(MovimentacaoCreditoLoja movement)
    {
        return new
        {
            movement.Id,
            movement.ContaCreditoLojaId,
            movement.TipoMovimentacao,
            movement.OrigemTipo,
            movement.OrigemId,
            movement.Valor,
            movement.SaldoAnterior,
            movement.SaldoPosterior,
            movement.Observacoes,
            movement.MovimentadoEm,
            movement.MovimentadoPorUsuarioId,
        };
    }

    // Modela a linha validada da liquidacao antes da persistencia.
    private sealed record NormalizedSettlementPayment(
        string TipoLiquidacao,
        Guid? MeioPagamentoId,
        decimal Valor);
}
