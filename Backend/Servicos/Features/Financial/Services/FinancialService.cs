using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Financial.Abstractions;
using Renova.Services.Features.Financial.Contracts;

namespace Renova.Services.Features.Financial.Services;

// Implementa o modulo 12 com livro razao, conciliacao e lancamentos avulsos.
public sealed class FinancialService : IFinancialService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public FinancialService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega meios de pagamento e filtros basicos da loja ativa.
    /// </summary>
    public async Task<FinancialWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureFinancialViewContextAsync(cancellationToken);
        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var paymentMethods = await _dbContext.MeiosPagamento
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId && x.Ativo)
            .OrderBy(x => x.Nome)
            .Select(x => new FinancialPaymentMethodOptionResponse(
                x.Id,
                x.Nome,
                x.TipoMeioPagamento,
                FinancialValues.GetPaymentMethodTypeLabel(x.TipoMeioPagamento),
                x.TaxaPercentual,
                x.PrazoRecebimentoDias))
            .ToListAsync(cancellationToken);

        return new FinancialWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            paymentMethods,
            FinancialValues.BuildMovementTypeOptions()
                .Select(x => new FinancialOptionResponse(x.Codigo, x.Nome))
                .ToArray(),
            FinancialValues.BuildManualMovementTypeOptions()
                .Select(x => new FinancialOptionResponse(x.Codigo, x.Nome))
                .ToArray(),
            FinancialValues.BuildDirectionOptions()
                .Select(x => new FinancialOptionResponse(x.Codigo, x.Nome))
                .ToArray());
    }

    /// <summary>
    /// Lista o livro razao financeiro com filtros por periodo, meio e tipo.
    /// </summary>
    public async Task<IReadOnlyList<FinancialLedgerEntryResponse>> ListarAsync(
        FinancialListQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureFinancialViewContextAsync(cancellationToken);
        var rows = await LoadLedgerRowsAsync(context.LojaId, query, cancellationToken);
        return rows.Select(MapLedgerEntry).ToArray();
    }

    /// <summary>
    /// Consolida o financeiro da loja ativa por meio, tipo e dia.
    /// </summary>
    public async Task<FinancialReconciliationResponse> ObterConciliacaoAsync(
        FinancialListQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureFinancialViewContextAsync(cancellationToken);
        var rows = await LoadLedgerRowsAsync(context.LojaId, query, cancellationToken);

        var movementTypeOptions = FinancialValues.BuildMovementTypeOptions();
        var byPaymentMethod = rows
            .GroupBy(x => new
            {
                Codigo = x.MeioPagamentoId?.ToString() ?? "sem_meio_pagamento",
                Nome = string.IsNullOrWhiteSpace(x.MeioPagamentoNome)
                    ? "Sem meio de pagamento"
                    : x.MeioPagamentoNome!,
            })
            .OrderBy(x => x.Key.Nome)
            .Select(group => BuildBreakdown(group.Key.Codigo, group.Key.Nome, group))
            .ToArray();

        var byMovementType = rows
            .GroupBy(x => x.TipoMovimentacao)
            .OrderBy(x => x.Key)
            .Select(group =>
            {
                var option = movementTypeOptions.FirstOrDefault(x => x.Codigo == group.Key);
                return BuildBreakdown(group.Key, option.Nome ?? group.Key, group);
            })
            .ToArray();

        var dailySummary = rows
            .GroupBy(x => x.MovimentadoEm.Date)
            .OrderByDescending(x => x.Key)
            .Select(group =>
            {
                var aggregate = BuildAggregate(group);
                return new FinancialDailySummaryResponse(
                    group.Key.ToString("yyyy-MM-dd"),
                    aggregate.QuantidadeLancamentos,
                    aggregate.TotalEntradasBrutas,
                    aggregate.TotalSaidasBrutas,
                    aggregate.SaldoBruto,
                    aggregate.TotalEntradasLiquidas,
                    aggregate.TotalSaidasLiquidas,
                    aggregate.SaldoLiquido,
                    aggregate.TotalTaxas);
            })
            .ToArray();

        return new FinancialReconciliationResponse(
            BuildAggregate(rows),
            byPaymentMethod,
            byMovementType,
            dailySummary);
    }

    /// <summary>
    /// Registra um movimento financeiro avulso na loja ativa.
    /// </summary>
    public async Task<FinancialLedgerEntryResponse> RegistrarLancamentoAsync(
        RegisterFinancialEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureFinancialManageContextAsync(cancellationToken);
        var movementType = FinancialValues.NormalizeManualMovementType(request.TipoMovimentacao);
        var direction = FinancialValues.NormalizeManualDirection(movementType, request.Direcao);
        var description = NormalizeRequiredText(
            request.Descricao,
            "Informe a descricao do lancamento financeiro.");
        var grossValue = RoundMoney(request.ValorBruto);
        if (grossValue <= 0m)
        {
            throw new InvalidOperationException("Informe um valor bruto maior que zero.");
        }

        var fee = RoundMoney(request.Taxa);
        if (fee < 0m)
        {
            throw new InvalidOperationException("A taxa do lancamento nao pode ser negativa.");
        }

        MeioPagamento? paymentMethod = null;
        if (request.MeioPagamentoId.HasValue)
        {
            paymentMethod = await _dbContext.MeiosPagamento
                .FirstOrDefaultAsync(
                    x => x.Id == request.MeioPagamentoId.Value &&
                         x.LojaId == context.LojaId &&
                         x.Ativo,
                    cancellationToken)
                ?? throw new InvalidOperationException("Meio de pagamento nao encontrado na loja ativa.");
        }

        var movedAt = ToUtcDate(request.MovimentadoEm) ?? DateTimeOffset.UtcNow;
        var competence = ToUtcDate(request.CompetenciaEm) ?? movedAt;
        var entity = new MovimentacaoFinanceira
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            TipoMovimentacao = movementType,
            Direcao = direction,
            MeioPagamentoId = paymentMethod?.Id,
            ValorBruto = grossValue,
            Taxa = fee,
            ValorLiquido = ComputeNetValue(direction, grossValue, fee),
            Descricao = description,
            CompetenciaEm = competence,
            MovimentadoEm = movedAt,
            MovimentadoPorUsuarioId = context.UsuarioId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.MovimentacoesFinanceiras.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "movimentacao_financeira",
            entity.Id,
            "criada",
            null,
            SnapshotFinancialMovement(entity, paymentMethod?.Nome),
            cancellationToken);

        var row = await LoadLedgerRowByIdAsync(context.LojaId, entity.Id, cancellationToken)
            ?? throw new InvalidOperationException("Lancamento financeiro criado, mas nao foi possivel recarregar o detalhe.");

        return MapLedgerEntry(row);
    }

    /// <summary>
    /// Recarrega um lancamento especifico do livro razao.
    /// </summary>
    private async Task<FinancialLedgerRow?> LoadLedgerRowByIdAsync(
        Guid lojaId,
        Guid movementId,
        CancellationToken cancellationToken)
    {
        var query = CreateLedgerQuery(lojaId);
        return await query.FirstOrDefaultAsync(x => x.Id == movementId, cancellationToken);
    }

    /// <summary>
    /// Monta a consulta base com todas as referencias relevantes da conciliacao.
    /// </summary>
    private IQueryable<FinancialLedgerRow> CreateLedgerQuery(Guid lojaId)
    {
        return
            from movement in _dbContext.MovimentacoesFinanceiras.AsNoTracking()
            join user in _dbContext.Usuarios.AsNoTracking() on movement.MovimentadoPorUsuarioId equals user.Id
            join paymentMethod in _dbContext.MeiosPagamento.AsNoTracking() on movement.MeioPagamentoId equals paymentMethod.Id into paymentMethodGroup
            from paymentMethod in paymentMethodGroup.DefaultIfEmpty()
            join salePayment in _dbContext.VendaPagamentos.AsNoTracking() on movement.VendaPagamentoId equals salePayment.Id into salePaymentGroup
            from salePayment in salePaymentGroup.DefaultIfEmpty()
            join sale in _dbContext.Vendas.AsNoTracking() on salePayment.VendaId equals sale.Id into saleGroup
            from sale in saleGroup.DefaultIfEmpty()
            join liquidation in _dbContext.LiquidacoesObrigacaoFornecedor.AsNoTracking() on movement.LiquidacaoObrigacaoFornecedorId equals liquidation.Id into liquidationGroup
            from liquidation in liquidationGroup.DefaultIfEmpty()
            join obligation in _dbContext.ObrigacoesFornecedor.AsNoTracking() on liquidation.ObrigacaoFornecedorId equals obligation.Id into obligationGroup
            from obligation in obligationGroup.DefaultIfEmpty()
            join supplier in _dbContext.Pessoas.AsNoTracking() on obligation.PessoaId equals supplier.Id into supplierGroup
            from supplier in supplierGroup.DefaultIfEmpty()
            where movement.LojaId == lojaId
            select new FinancialLedgerRow
            {
                Id = movement.Id,
                TipoMovimentacao = movement.TipoMovimentacao,
                Direcao = movement.Direcao,
                MeioPagamentoId = movement.MeioPagamentoId,
                MeioPagamentoNome = paymentMethod != null ? paymentMethod.Nome : null,
                VendaId = sale != null ? sale.Id : null,
                NumeroVenda = sale != null ? sale.NumeroVenda : null,
                LiquidacaoObrigacaoFornecedorId = movement.LiquidacaoObrigacaoFornecedorId,
                ObrigacaoFornecedorId = obligation != null ? obligation.Id : null,
                FornecedorNome = supplier != null ? supplier.Nome : null,
                ValorBruto = movement.ValorBruto,
                Taxa = movement.Taxa,
                ValorLiquido = movement.ValorLiquido,
                Descricao = movement.Descricao,
                CompetenciaEm = movement.CompetenciaEm,
                MovimentadoEm = movement.MovimentadoEm,
                MovimentadoPorUsuarioId = movement.MovimentadoPorUsuarioId,
                MovimentadoPorUsuarioNome = user.Nome,
                CriadoEm = movement.CriadoEm,
            };
    }

    /// <summary>
    /// Aplica filtros simples e retorna as linhas do livro razao ordenadas.
    /// </summary>
    private async Task<List<FinancialLedgerRow>> LoadLedgerRowsAsync(
        Guid lojaId,
        FinancialListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var ledgerQuery = CreateLedgerQuery(lojaId);

        if (query.MeioPagamentoId.HasValue)
        {
            ledgerQuery = ledgerQuery.Where(x => x.MeioPagamentoId == query.MeioPagamentoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.TipoMovimentacao))
        {
            var movementType = FinancialValues.NormalizeMovementType(query.TipoMovimentacao);
            ledgerQuery = ledgerQuery.Where(x => x.TipoMovimentacao == movementType);
        }

        if (!string.IsNullOrWhiteSpace(query.Direcao))
        {
            var direction = FinancialValues.NormalizeDirection(query.Direcao);
            ledgerQuery = ledgerQuery.Where(x => x.Direcao == direction);
        }

        if (query.DataInicial.HasValue)
        {
            var start = ToUtcDate(query.DataInicial.Value);
            ledgerQuery = ledgerQuery.Where(x => x.MovimentadoEm >= start);
        }

        if (query.DataFinal.HasValue)
        {
            var endExclusive = ToUtcDate(query.DataFinal.Value.AddDays(1));
            ledgerQuery = ledgerQuery.Where(x => x.MovimentadoEm < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            ledgerQuery = ledgerQuery.Where(x =>
                x.TipoMovimentacao.ToLower().Contains(term) ||
                x.Descricao.ToLower().Contains(term) ||
                (x.MeioPagamentoNome ?? string.Empty).ToLower().Contains(term) ||
                (x.NumeroVenda ?? string.Empty).ToLower().Contains(term) ||
                (x.FornecedorNome ?? string.Empty).ToLower().Contains(term) ||
                x.MovimentadoPorUsuarioNome.ToLower().Contains(term));
        }

        return await ledgerQuery
            .OrderByDescending(x => x.MovimentadoEm)
            .ThenByDescending(x => x.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Converte a linha bruta do EF no contrato consumido pelo frontend.
    /// </summary>
    private static FinancialLedgerEntryResponse MapLedgerEntry(FinancialLedgerRow row)
    {
        return new FinancialLedgerEntryResponse(
            row.Id,
            row.TipoMovimentacao,
            row.Direcao,
            ResolveOriginType(row),
            row.MeioPagamentoId,
            row.MeioPagamentoNome,
            row.VendaId,
            row.NumeroVenda,
            row.LiquidacaoObrigacaoFornecedorId,
            row.ObrigacaoFornecedorId,
            row.FornecedorNome,
            row.ValorBruto,
            row.Taxa,
            row.ValorLiquido,
            row.Descricao,
            row.CompetenciaEm,
            row.MovimentadoEm,
            row.MovimentadoPorUsuarioId,
            row.MovimentadoPorUsuarioNome);
    }

    /// <summary>
    /// Resume entradas, saidas, liquido e taxas do conjunto filtrado.
    /// </summary>
    private static FinancialAggregateResponse BuildAggregate(IEnumerable<FinancialLedgerRow> rows)
    {
        var materialized = rows.ToArray();
        var totalEntradasBrutas = RoundMoney(materialized
            .Where(x => x.Direcao == FinancialValues.Directions.Entrada)
            .Sum(x => x.ValorBruto));
        var totalSaidasBrutas = RoundMoney(materialized
            .Where(x => x.Direcao == FinancialValues.Directions.Saida)
            .Sum(x => x.ValorBruto));
        var totalEntradasLiquidas = RoundMoney(materialized
            .Where(x => x.Direcao == FinancialValues.Directions.Entrada)
            .Sum(x => x.ValorLiquido));
        var totalSaidasLiquidas = RoundMoney(materialized
            .Where(x => x.Direcao == FinancialValues.Directions.Saida)
            .Sum(x => x.ValorLiquido));
        var totalTaxas = RoundMoney(materialized.Sum(x => x.Taxa));

        return new FinancialAggregateResponse(
            materialized.Length,
            totalEntradasBrutas,
            totalSaidasBrutas,
            RoundMoney(totalEntradasBrutas - totalSaidasBrutas),
            totalEntradasLiquidas,
            totalSaidasLiquidas,
            RoundMoney(totalEntradasLiquidas - totalSaidasLiquidas),
            totalTaxas);
    }

    /// <summary>
    /// Construi um bloco de conciliacao agregada com o mesmo formato do resumo geral.
    /// </summary>
    private static FinancialBreakdownResponse BuildBreakdown(
        string code,
        string name,
        IEnumerable<FinancialLedgerRow> rows)
    {
        var aggregate = BuildAggregate(rows);
        return new FinancialBreakdownResponse(
            code,
            name,
            aggregate.QuantidadeLancamentos,
            aggregate.TotalEntradasBrutas,
            aggregate.TotalSaidasBrutas,
            aggregate.SaldoBruto,
            aggregate.TotalEntradasLiquidas,
            aggregate.TotalSaidasLiquidas,
            aggregate.SaldoLiquido,
            aggregate.TotalTaxas);
    }

    /// <summary>
    /// Determina se o movimento veio de venda, fornecedor ou cadastro avulso.
    /// </summary>
    private static string ResolveOriginType(FinancialLedgerRow row)
    {
        if (row.VendaId.HasValue)
        {
            return FinancialValues.OriginTypes.Venda;
        }

        if (row.LiquidacaoObrigacaoFornecedorId.HasValue)
        {
            return FinancialValues.OriginTypes.ObrigacaoFornecedor;
        }

        return FinancialValues.OriginTypes.Avulso;
    }

    /// <summary>
    /// Gera o valor liquido conforme a direcao financeira.
    /// </summary>
    private static decimal ComputeNetValue(string direction, decimal grossValue, decimal fee)
    {
        return direction == FinancialValues.Directions.Entrada
            ? RoundMoney(grossValue - fee)
            : RoundMoney(grossValue + fee);
    }

    /// <summary>
    /// Normaliza textos obrigatorios para lancamentos e mensagens.
    /// </summary>
    private static string NormalizeRequiredText(string? value, string errorMessage)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return normalized;
    }

    /// <summary>
    /// Converte uma data simples da UI em um ponto UTC padrao.
    /// </summary>
    private static DateTimeOffset ToUtcDate(DateOnly value)
    {
        return new DateTimeOffset(value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
    }

    /// <summary>
    /// Converte datas opcionais sem obrigar preenchimento no frontend.
    /// </summary>
    private static DateTimeOffset? ToUtcDate(DateOnly? value)
    {
        return value.HasValue ? ToUtcDate(value.Value) : null;
    }

    /// <summary>
    /// Padroniza valores monetarios antes de salvar ou agregar.
    /// </summary>
    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Garante contexto autenticado com acesso financeiro de leitura.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureFinancialViewContextAsync(CancellationToken cancellationToken)
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
            throw new InvalidOperationException("Voce nao possui acesso ao financeiro da loja ativa.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Exige permissao explicita para lancamentos e conciliacao.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureFinancialManageContextAsync(CancellationToken cancellationToken)
    {
        var context = await EnsureFinancialViewContextAsync(cancellationToken);
        var hasPermission = await HasPermissionAsync(
            context.UsuarioId,
            context.LojaId,
            [AccessPermissionCodes.FinanceiroConciliar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui permissao para conciliar o financeiro.");
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
    /// Verifica se o usuario possui alguma permissao da lista no contexto da loja.
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
    /// Resume o payload auditavel do movimento financeiro criado manualmente.
    /// </summary>
    private static object SnapshotFinancialMovement(MovimentacaoFinanceira entity, string? paymentMethodName)
    {
        return new
        {
            entity.LojaId,
            entity.TipoMovimentacao,
            entity.Direcao,
            entity.MeioPagamentoId,
            MeioPagamentoNome = paymentMethodName,
            entity.ValorBruto,
            entity.Taxa,
            entity.ValorLiquido,
            entity.Descricao,
            entity.CompetenciaEm,
            entity.MovimentadoEm,
            entity.MovimentadoPorUsuarioId,
        };
    }

    // Estrutura interna para transportar a linha do livro razao entre consulta e mapeamento.
    private sealed class FinancialLedgerRow
    {
        public Guid Id { get; init; }
        public string TipoMovimentacao { get; init; } = string.Empty;
        public string Direcao { get; init; } = string.Empty;
        public Guid? MeioPagamentoId { get; init; }
        public string? MeioPagamentoNome { get; init; }
        public Guid? VendaId { get; init; }
        public string? NumeroVenda { get; init; }
        public Guid? LiquidacaoObrigacaoFornecedorId { get; init; }
        public Guid? ObrigacaoFornecedorId { get; init; }
        public string? FornecedorNome { get; init; }
        public decimal ValorBruto { get; init; }
        public decimal Taxa { get; init; }
        public decimal ValorLiquido { get; init; }
        public string Descricao { get; init; } = string.Empty;
        public DateTimeOffset? CompetenciaEm { get; init; }
        public DateTimeOffset MovimentadoEm { get; init; }
        public Guid MovimentadoPorUsuarioId { get; init; }
        public string MovimentadoPorUsuarioNome { get; init; } = string.Empty;
        public DateTimeOffset CriadoEm { get; init; }
    }
}
