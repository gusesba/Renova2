using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Services.Features.Access;
using Renova.Services.Features.Consignments;
using Renova.Services.Features.People;
using Renova.Services.Features.Pieces;
using Renova.Services.Features.Sales.Contracts;
using Renova.Services.Features.SupplierPayments;

namespace Renova.Services.Features.Sales.Services;

// Complementa o service com regras privadas, cancelamento e mapeamentos do modulo 09.
public sealed partial class SaleService
{
    /// <summary>
    /// Cancela a venda, estorna saldo e registra movimentos reversos.
    /// </summary>
    public async Task<SaleDetailResponse> CancelarAsync(
        Guid vendaId,
        CancelSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureSalesCancelContextAsync(cancellationToken);
        var aggregate = await LoadSaleAggregateAsync(context.LojaId, vendaId, cancellationToken, tracking: true)
            ?? throw new InvalidOperationException("Venda nao encontrada na loja ativa.");

        if (aggregate.Venda.StatusVenda == SaleValues.SaleStatuses.Cancelada)
        {
            throw new InvalidOperationException("A venda informada ja esta cancelada.");
        }

        var cancelReason = NormalizeRequiredText(
            request.MotivoCancelamento,
            "Informe o motivo do cancelamento da venda.");

        var pieceIds = aggregate.Itens.Select(x => x.Item.PecaId).Distinct().ToArray();
        var pieces = await _dbContext.Pecas
            .Where(x => pieceIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var paymentIds = aggregate.Pagamentos.Select(x => x.Pagamento.Id).ToArray();
        var financialMovements = await _dbContext.MovimentacoesFinanceiras
            .Where(x => x.LojaId == context.LojaId)
            .Where(x => paymentIds.Contains(x.VendaPagamentoId ?? Guid.Empty))
            .ToListAsync(cancellationToken);

        var creditMovements = await (
                from movement in _dbContext.MovimentacoesCreditoLoja
                join account in _dbContext.ContasCreditoLoja on movement.ContaCreditoLojaId equals account.Id
                where account.LojaId == context.LojaId
                where movement.OrigemTipo == SaleValues.CreditOrigins.Venda
                where movement.OrigemId == aggregate.Venda.Id
                select new { Movement = movement, Account = account })
            .ToListAsync(cancellationToken);

        var obligationIds = aggregate.Itens.Select(x => x.Item.Id).ToArray();
        var obligations = await _dbContext.ObrigacoesFornecedor
            .Where(x => x.LojaId == context.LojaId)
            .Where(x => obligationIds.Contains(x.VendaItemId ?? Guid.Empty))
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        aggregate.Venda.StatusVenda = SaleValues.SaleStatuses.Cancelada;
        aggregate.Venda.CanceladaEm = now;
        aggregate.Venda.CanceladaPorUsuarioId = context.UsuarioId;
        aggregate.Venda.MotivoCancelamento = cancelReason;
        TouchEntity(aggregate.Venda, context.UsuarioId);

        var stockReversals = new List<MovimentacaoEstoque>();
        foreach (var item in aggregate.Itens)
        {
            if (!pieces.TryGetValue(item.Item.PecaId, out var piece))
            {
                throw new InvalidOperationException("Peca da venda nao encontrada para estorno.");
            }

            var previousBalance = piece.QuantidadeAtual;
            piece.QuantidadeAtual += item.Item.Quantidade;
            piece.StatusPeca = piece.QuantidadeAtual > 0
                ? PieceValues.PieceStatuses.Disponivel
                : piece.StatusPeca;
            TouchEntity(piece, context.UsuarioId);

            stockReversals.Add(new MovimentacaoEstoque
            {
                Id = Guid.NewGuid(),
                LojaId = context.LojaId,
                PecaId = piece.Id,
                TipoMovimentacao = PieceValues.StockMovementTypes.CancelamentoVenda,
                Quantidade = item.Item.Quantidade,
                SaldoAnterior = previousBalance,
                SaldoPosterior = piece.QuantidadeAtual,
                OrigemTipo = PieceValues.StockOrigins.Venda,
                OrigemId = aggregate.Venda.Id,
                Motivo = $"Cancelamento da venda {aggregate.Venda.NumeroVenda}",
                MovimentadoEm = now,
                MovimentadoPorUsuarioId = context.UsuarioId,
                CriadoPorUsuarioId = context.UsuarioId,
            });
        }

        var financialReversals = new List<MovimentacaoFinanceira>();
        foreach (var movement in financialMovements)
        {
            financialReversals.Add(new MovimentacaoFinanceira
            {
                Id = Guid.NewGuid(),
                LojaId = context.LojaId,
                TipoMovimentacao = SaleValues.FinancialMovementTypes.EstornoVenda,
                Direcao = SaleValues.FinancialDirections.Saida,
                MeioPagamentoId = movement.MeioPagamentoId,
                VendaPagamentoId = movement.VendaPagamentoId,
                ValorBruto = movement.ValorBruto,
                Taxa = movement.Taxa,
                ValorLiquido = movement.ValorLiquido,
                Descricao = $"Estorno da venda {aggregate.Venda.NumeroVenda}.",
                CompetenciaEm = now,
                MovimentadoEm = now,
                MovimentadoPorUsuarioId = context.UsuarioId,
                CriadoPorUsuarioId = context.UsuarioId,
            });
        }

        var creditReversals = new List<MovimentacaoCreditoLoja>();
        foreach (var creditMovement in creditMovements)
        {
            var previousBalance = creditMovement.Account.SaldoAtual;
            creditMovement.Account.SaldoAtual = RoundMoney(previousBalance + creditMovement.Movement.Valor);
            TouchEntity(creditMovement.Account, context.UsuarioId);

            creditReversals.Add(new MovimentacaoCreditoLoja
            {
                Id = Guid.NewGuid(),
                ContaCreditoLojaId = creditMovement.Account.Id,
                TipoMovimentacao = SaleValues.CreditMovementTypes.EstornoCreditoVenda,
                OrigemTipo = SaleValues.CreditOrigins.Venda,
                OrigemId = aggregate.Venda.Id,
                Valor = creditMovement.Movement.Valor,
                SaldoAnterior = previousBalance,
                SaldoPosterior = creditMovement.Account.SaldoAtual,
                Observacoes = $"Estorno de credito da venda {aggregate.Venda.NumeroVenda}.",
                MovimentadoEm = now,
                MovimentadoPorUsuarioId = context.UsuarioId,
                CriadoPorUsuarioId = context.UsuarioId,
            });
        }

        foreach (var obligation in obligations)
        {
            obligation.StatusObrigacao = SupplierPaymentValues.ObligationStatuses.Cancelada;
            obligation.ValorEmAberto = 0m;
            obligation.Observacoes = string.IsNullOrWhiteSpace(obligation.Observacoes)
                ? $"Obrigacao cancelada pelo estorno da venda {aggregate.Venda.NumeroVenda}."
                : $"{obligation.Observacoes} Cancelada pelo estorno da venda {aggregate.Venda.NumeroVenda}.";
            TouchEntity(obligation, context.UsuarioId);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.MovimentacoesEstoque.AddRange(stockReversals);
        _dbContext.MovimentacoesFinanceiras.AddRange(financialReversals);
        _dbContext.MovimentacoesCreditoLoja.AddRange(creditReversals);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await RegisterCancelAuditsAsync(
            context.LojaId,
            aggregate.Venda,
            stockReversals,
            financialReversals,
            creditReversals,
            obligations,
            cancelReason,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return await ObterDetalheAsync(aggregate.Venda.Id, cancellationToken);
    }

    /// <summary>
    /// Exige autenticao, loja ativa e algum acesso operacional ao modulo de vendas.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureSalesViewContextAsync(CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para continuar.");

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

        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.VendasRegistrar, AccessPermissionCodes.VendasCancelar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui acesso ao modulo de vendas na loja ativa.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Exige permissao explicita para registrar novas vendas.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureSalesManageContextAsync(CancellationToken cancellationToken)
    {
        var context = await EnsureSalesViewContextAsync(cancellationToken);
        var hasPermission = await HasPermissionAsync(
            context.UsuarioId,
            context.LojaId,
            [AccessPermissionCodes.VendasRegistrar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui permissao para registrar vendas.");
        }

        return context;
    }

    /// <summary>
    /// Exige permissao explicita para cancelar vendas concluidas.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureSalesCancelContextAsync(CancellationToken cancellationToken)
    {
        var context = await EnsureSalesViewContextAsync(cancellationToken);
        var hasPermission = await HasPermissionAsync(
            context.UsuarioId,
            context.LojaId,
            [AccessPermissionCodes.VendasCancelar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui permissao para cancelar vendas.");
        }

        return context;
    }

    /// <summary>
    /// Verifica se o usuario possui ao menos uma permissao na loja ativa.
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
    /// Garante que o comprador informado esta ativo na loja e pode participar da venda.
    /// </summary>
    private async Task<BuyerContext> LoadBuyerContextAsync(Guid lojaId, Guid pessoaId, CancellationToken cancellationToken)
    {
        var buyer = await (
                from pessoaLoja in _dbContext.PessoaLojas
                join pessoa in _dbContext.Pessoas on pessoaLoja.PessoaId equals pessoa.Id
                where pessoaLoja.LojaId == lojaId
                where pessoaLoja.PessoaId == pessoaId
                select new { Relacao = pessoaLoja, Pessoa = pessoa })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Comprador nao encontrado na loja ativa.");

        if (!buyer.Relacao.EhCliente || buyer.Relacao.StatusRelacao != PeopleStatusValues.StatusRelacao.Ativo)
        {
            throw new InvalidOperationException("O comprador informado nao esta ativo como cliente na loja.");
        }

        return new BuyerContext(buyer.Pessoa, buyer.Relacao);
    }

    /// <summary>
    /// Carrega as pecas solicitadas com o snapshot comercial necessario para a venda.
    /// </summary>
    private async Task<IReadOnlyList<SalePieceInput>> LoadSalePieceInputsAsync(
        Guid lojaId,
        IReadOnlyCollection<Guid> pieceIds,
        CancellationToken cancellationToken)
    {
        var items = await (
                from peca in _dbContext.Pecas
                join produto in _dbContext.ProdutoNomes on peca.ProdutoNomeId equals produto.Id
                join marca in _dbContext.Marcas on peca.MarcaId equals marca.Id
                join tamanho in _dbContext.Tamanhos on peca.TamanhoId equals tamanho.Id
                join cor in _dbContext.Cores on peca.CorId equals cor.Id
                join condicao in _dbContext.PecaCondicoesComerciais on peca.Id equals condicao.PecaId into conditionGroup
                from condicao in conditionGroup.DefaultIfEmpty()
                join fornecedor in _dbContext.Pessoas on peca.FornecedorPessoaId equals fornecedor.Id into supplierGroup
                from fornecedor in supplierGroup.DefaultIfEmpty()
                where peca.LojaId == lojaId
                where pieceIds.Contains(peca.Id)
                select new SalePieceInput(
                    peca,
                    produto.Nome,
                    marca.Nome,
                    tamanho.Nome,
                    cor.Nome,
                    fornecedor != null ? fornecedor.Nome : null,
                    condicao,
                    peca.PrecoVendaAtual,
                    peca.PrecoVendaAtual,
                    0m,
                    false))
            .ToListAsync(cancellationToken);

        var basePriceMap = await LoadBasePriceMapAsync(pieceIds, cancellationToken);

        return items
            .Select(item =>
            {
                var basePrice = basePriceMap.TryGetValue(item.Peca.Id, out var loadedBasePrice)
                    ? loadedBasePrice
                    : item.Peca.PrecoVendaAtual;
                var lifecycle = ConsignmentLifecycleCalculator.Calculate(item.Peca, item.CondicaoComercial, basePrice);

                return item with
                {
                    PrecoBase = lifecycle.PrecoBase,
                    PrecoEfetivoVenda = lifecycle.PrecoEfetivoVenda,
                    PercentualDescontoAutomatico = lifecycle.PercentualDescontoEsperado,
                    DescontoAutomaticoAtivo = lifecycle.DescontoAutomaticoAtivo,
                };
            })
            .ToList();
    }

    /// <summary>
    /// Carrega o preco base de referencia das pecas para evitar desconto cumulativo persistido em consignacao.
    /// </summary>
    private async Task<IReadOnlyDictionary<Guid, decimal>> LoadBasePriceMapAsync(
        IReadOnlyCollection<Guid> pieceIds,
        CancellationToken cancellationToken)
    {
        if (pieceIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var currentPrices = await _dbContext.Pecas
            .AsNoTracking()
            .Where(x => pieceIds.Contains(x.Id))
            .Select(x => new { x.Id, x.PrecoVendaAtual })
            .ToListAsync(cancellationToken);

        var historyItems = await _dbContext.PecaHistoricosPreco
            .AsNoTracking()
            .Where(x => pieceIds.Contains(x.PecaId))
            .OrderBy(x => x.AlteradoEm)
            .ThenBy(x => x.CriadoEm)
            .ToListAsync(cancellationToken);

        var result = currentPrices.ToDictionary(x => x.Id, x => x.PrecoVendaAtual);
        foreach (var group in historyItems.GroupBy(x => x.PecaId))
        {
            result[group.Key] = group.First().PrecoAnterior;
        }

        return result;
    }

    /// <summary>
    /// Calcula os itens normalizados da venda a partir das pecas e dos pagamentos informados.
    /// </summary>
    private static IReadOnlyList<ComputedSaleItem> BuildSaleItems(
        IReadOnlyList<CreateSaleItemRequest> itemRequests,
        IReadOnlyList<SalePieceInput> pieces,
        IReadOnlyList<NormalizedPaymentInput> normalizedPayments)
    {
        var pieceMap = pieces.ToDictionary(x => x.Peca.Id, x => x);
        var totalPagamentos = RoundMoney(normalizedPayments.Sum(x => x.Valor));
        var totalFinanceiro = RoundMoney(normalizedPayments
            .Where(x => x.TipoPagamento == SaleValues.PaymentTypes.MeioPagamento)
            .Sum(x => x.Valor));
        var totalCredito = RoundMoney(normalizedPayments
            .Where(x => x.TipoPagamento == SaleValues.PaymentTypes.CreditoLoja)
            .Sum(x => x.Valor));
        var percentualFinanceiro = totalPagamentos <= 0m ? 0m : totalFinanceiro / totalPagamentos;
        var percentualCredito = totalPagamentos <= 0m ? 0m : totalCredito / totalPagamentos;

        var items = new List<ComputedSaleItem>();
        foreach (var request in itemRequests)
        {
            if (!pieceMap.TryGetValue(request.PecaId, out var piece))
            {
                throw new InvalidOperationException("Peca da venda nao encontrada.");
            }

            if (request.Quantidade <= 0)
            {
                throw new InvalidOperationException("Informe uma quantidade maior que zero para cada item.");
            }

            var discountUnit = RoundMoney(request.DescontoUnitario);
            if (discountUnit < 0m)
            {
                throw new InvalidOperationException("O desconto do item nao pode ser negativo.");
            }

            if (discountUnit > piece.PrecoEfetivoVenda)
            {
                throw new InvalidOperationException($"O desconto da peca {piece.Peca.CodigoInterno} nao pode superar o preco.");
            }

            var finalUnitPrice = RoundMoney(piece.PrecoEfetivoVenda - discountUnit);
            var itemTotal = RoundMoney(finalUnitPrice * request.Quantidade);
            var percentualDinheiro = piece.CondicaoComercial?.PercentualRepasseDinheiro ?? 0m;
            var percentualCreditoItem = piece.CondicaoComercial?.PercentualRepasseCredito ?? 0m;
            var repasseDinheiro = RoundMoney(itemTotal * percentualFinanceiro * (percentualDinheiro / 100m));
            var repasseCredito = RoundMoney(itemTotal * percentualCredito * (percentualCreditoItem / 100m));

            items.Add(new ComputedSaleItem(
                piece.Peca.Id,
                piece.Peca.CodigoInterno,
                request.Quantidade,
                piece.PrecoEfetivoVenda,
                discountUnit,
                finalUnitPrice,
                percentualDinheiro,
                percentualCreditoItem,
                RoundMoney(repasseDinheiro + repasseCredito),
                piece.CondicaoComercial?.PermitePagamentoMisto ?? false));
        }

        return items;
    }

    /// <summary>
    /// Carrega o agregado completo da venda para detalhe e cancelamento.
    /// </summary>
    private async Task<SaleAggregate?> LoadSaleAggregateAsync(
        Guid lojaId,
        Guid vendaId,
        CancellationToken cancellationToken,
        bool tracking = false)
    {
        var sales = tracking ? _dbContext.Vendas : _dbContext.Vendas.AsNoTracking();
        var users = tracking ? _dbContext.Usuarios : _dbContext.Usuarios.AsNoTracking();
        var people = tracking ? _dbContext.Pessoas : _dbContext.Pessoas.AsNoTracking();
        var items = tracking ? _dbContext.VendaItens : _dbContext.VendaItens.AsNoTracking();
        var pieces = tracking ? _dbContext.Pecas : _dbContext.Pecas.AsNoTracking();
        var products = tracking ? _dbContext.ProdutoNomes : _dbContext.ProdutoNomes.AsNoTracking();
        var brands = tracking ? _dbContext.Marcas : _dbContext.Marcas.AsNoTracking();
        var sizes = tracking ? _dbContext.Tamanhos : _dbContext.Tamanhos.AsNoTracking();
        var colors = tracking ? _dbContext.Cores : _dbContext.Cores.AsNoTracking();
        var payments = tracking ? _dbContext.VendaPagamentos : _dbContext.VendaPagamentos.AsNoTracking();
        var methods = tracking ? _dbContext.MeiosPagamento : _dbContext.MeiosPagamento.AsNoTracking();

        var sale = await (
                from venda in sales
                join vendedor in users on venda.VendedorUsuarioId equals vendedor.Id
                join comprador in people on venda.CompradorPessoaId equals comprador.Id into buyerGroup
                from comprador in buyerGroup.DefaultIfEmpty()
                where venda.LojaId == lojaId && venda.Id == vendaId
                select new
                {
                    Venda = venda,
                    VendedorNome = vendedor.Nome,
                    CompradorNome = comprador != null ? comprador.Nome : null,
                })
            .FirstOrDefaultAsync(cancellationToken);

        if (sale is null)
        {
            return null;
        }

        var itemRows = await (
                from item in items
                join piece in pieces on item.PecaId equals piece.Id
                join product in products on piece.ProdutoNomeId equals product.Id
                join brand in brands on piece.MarcaId equals brand.Id
                join size in sizes on piece.TamanhoId equals size.Id
                join color in colors on piece.CorId equals color.Id
                join supplier in people on item.FornecedorPessoaIdSnapshot equals supplier.Id into supplierGroup
                from supplier in supplierGroup.DefaultIfEmpty()
                where item.VendaId == vendaId
                orderby item.CriadoEm
                select new SaleAggregateItem(
                    item,
                    piece.CodigoInterno,
                    product.Nome,
                    brand.Nome,
                    size.Nome,
                    color.Nome,
                    supplier != null ? supplier.Nome : null))
            .ToListAsync(cancellationToken);

        var paymentRows = await (
                from payment in payments
                join method in methods on payment.MeioPagamentoId equals method.Id into methodGroup
                from method in methodGroup.DefaultIfEmpty()
                where payment.VendaId == vendaId
                orderby payment.Sequencia
                select new SaleAggregatePayment(
                    payment,
                    method != null ? method.Nome : null))
            .ToListAsync(cancellationToken);

        return new SaleAggregate(
            sale.Venda,
            sale.VendedorNome,
            sale.CompradorNome,
            itemRows,
            paymentRows);
    }

    /// <summary>
    /// Registra auditoria resumida dos artefatos criados na conclusao da venda.
    /// </summary>
    private async Task RegisterCreateAuditsAsync(
        Guid lojaId,
        Venda sale,
        IReadOnlyList<MovimentacaoEstoque> stockMovements,
        IReadOnlyList<VendaPagamento> payments,
        IReadOnlyList<MovimentacaoFinanceira> financialMovements,
        IReadOnlyList<MovimentacaoCreditoLoja> creditMovements,
        IReadOnlyList<ObrigacaoFornecedor> obligations,
        CancellationToken cancellationToken)
    {
        await _auditService.RegistrarAuditoriaAsync(
            lojaId,
            "venda",
            sale.Id,
            "criar",
            null,
            new
            {
                sale.NumeroVenda,
                sale.StatusVenda,
                sale.CompradorPessoaId,
                sale.Subtotal,
                sale.DescontoTotal,
                sale.TaxaTotal,
                sale.TotalLiquido,
                Pagamentos = payments.Count,
                MovimentosEstoque = stockMovements.Count,
                MovimentosFinanceiros = financialMovements.Count,
                MovimentosCredito = creditMovements.Count,
                ObrigacoesFornecedor = obligations.Count,
            },
            cancellationToken);
    }

    /// <summary>
    /// Registra auditoria resumida dos estornos aplicados no cancelamento.
    /// </summary>
    private async Task RegisterCancelAuditsAsync(
        Guid lojaId,
        Venda sale,
        IReadOnlyList<MovimentacaoEstoque> stockReversals,
        IReadOnlyList<MovimentacaoFinanceira> financialReversals,
        IReadOnlyList<MovimentacaoCreditoLoja> creditReversals,
        IReadOnlyList<ObrigacaoFornecedor> obligations,
        string cancelReason,
        CancellationToken cancellationToken)
    {
        await _auditService.RegistrarAuditoriaAsync(
            lojaId,
            "venda",
            sale.Id,
            "cancelar",
            new
            {
                StatusVenda = SaleValues.SaleStatuses.Concluida,
            },
            new
            {
                sale.NumeroVenda,
                sale.StatusVenda,
                sale.CanceladaEm,
                sale.CanceladaPorUsuarioId,
                Motivo = cancelReason,
                EstornosEstoque = stockReversals.Count,
                EstornosFinanceiros = financialReversals.Count,
                EstornosCredito = creditReversals.Count,
                ObrigacoesCanceladas = obligations.Count,
            },
            cancellationToken);
    }

    /// <summary>
    /// Gera o numero sequencial usado no comprovante e rastreio da venda.
    /// </summary>
    private async Task<string> GenerateSaleNumberAsync(Guid lojaId, CancellationToken cancellationToken)
    {
        var prefix = $"VD-{DateTimeOffset.UtcNow:yyyyMMdd}-";

        var existingNumbers = await _dbContext.Vendas
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Where(x => x.NumeroVenda.StartsWith(prefix))
            .Select(x => x.NumeroVenda)
            .ToListAsync(cancellationToken);

        var nextSequence = existingNumbers
            .Select(number =>
            {
                var startIndex = number.LastIndexOf('-') + 1;
                var lastSegment = number[startIndex..];
                return int.TryParse(lastSegment, out var parsed) ? parsed : 0;
            })
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{prefix}{nextSequence:0000}";
    }

    /// <summary>
    /// Valida a estrutura minima da requisicao de criacao de venda.
    /// </summary>
    private static void ValidateCreateRequest(CreateSaleRequest request)
    {
        if (!request.CompradorPessoaId.HasValue)
        {
            throw new InvalidOperationException("Selecione o comprador da venda.");
        }

        if (request.Itens.Count == 0)
        {
            throw new InvalidOperationException("Adicione ao menos uma peca na venda.");
        }

        if (request.Pagamentos.Count == 0)
        {
            throw new InvalidOperationException("Informe ao menos um pagamento para concluir a venda.");
        }
    }

    /// <summary>
    /// Converte o agregado interno para o detalhe consumido pela API.
    /// </summary>
    private static SaleDetailResponse MapDetail(SaleAggregate aggregate)
    {
        var items = aggregate.Itens
            .Select(item => new SaleItemResponse(
                item.Item.Id,
                item.Item.PecaId,
                item.CodigoInterno,
                item.ProdutoNome,
                item.Marca,
                item.Tamanho,
                item.Cor,
                item.Item.Quantidade,
                item.Item.PrecoTabelaUnitario,
                item.Item.DescontoUnitario,
                item.Item.PrecoFinalUnitario,
                item.Item.TipoPecaSnapshot,
                item.Item.FornecedorPessoaIdSnapshot,
                item.FornecedorNome,
                item.Item.PercentualRepasseDinheiroSnapshot,
                item.Item.PercentualRepasseCreditoSnapshot,
                item.Item.ValorRepassePrevisto))
            .ToArray();

        var payments = aggregate.Pagamentos
            .Select(payment => new SalePaymentResponse(
                payment.Pagamento.Id,
                payment.Pagamento.Sequencia,
                payment.Pagamento.MeioPagamentoId,
                payment.MeioPagamentoNome,
                payment.Pagamento.TipoPagamento,
                payment.Pagamento.Valor,
                payment.Pagamento.TaxaPercentualAplicada,
                payment.Pagamento.ValorLiquido,
                payment.Pagamento.RecebidoEm))
            .ToArray();

        var detail = new SaleDetailResponse(
            aggregate.Venda.Id,
            aggregate.Venda.NumeroVenda,
            aggregate.Venda.StatusVenda,
            aggregate.Venda.DataHoraVenda,
            aggregate.Venda.CompradorPessoaId,
            aggregate.CompradorNome,
            aggregate.Venda.VendedorUsuarioId,
            aggregate.VendedorNome,
            aggregate.Venda.Subtotal,
            aggregate.Venda.DescontoTotal,
            aggregate.Venda.TaxaTotal,
            aggregate.Venda.TotalLiquido,
            aggregate.Venda.Observacoes,
            aggregate.Venda.CanceladaEm,
            aggregate.Venda.CanceladaPorUsuarioId,
            aggregate.Venda.MotivoCancelamento,
            items,
            payments,
            string.Empty);

        return detail with
        {
            ReciboTexto = BuildReceiptText(detail),
        };
    }

    /// <summary>
    /// Monta o texto simples do recibo da venda com itens e pagamentos.
    /// </summary>
    private static string BuildReceiptText(SaleDetailResponse detail)
    {
        var lines = new List<string>
        {
            "RECIBO DE VENDA",
            $"Numero: {detail.NumeroVenda}",
            $"Data: {detail.DataHoraVenda:dd/MM/yyyy HH:mm}",
            $"Status: {detail.StatusVenda}",
            $"Vendedor: {detail.VendedorNome}",
            $"Comprador: {detail.CompradorNome ?? "Nao informado"}",
            string.Empty,
            "ITENS",
        };

        foreach (var item in detail.Itens)
        {
            lines.Add(
                $"- {item.CodigoInterno} | {item.ProdutoNome} / {item.Marca} / {item.Cor} / {item.Tamanho} | Qtd {item.Quantidade} | Unit {FormatCurrency(item.PrecoFinalUnitario)}");
        }

        lines.Add(string.Empty);
        lines.Add("PAGAMENTOS");
        foreach (var payment in detail.Pagamentos)
        {
            lines.Add(
                $"- {payment.TipoPagamento} | {payment.MeioPagamentoNome ?? "Credito da loja"} | Valor {FormatCurrency(payment.Valor)} | Liquido {FormatCurrency(payment.ValorLiquido)}");
        }

        lines.Add(string.Empty);
        lines.Add($"Subtotal: {FormatCurrency(detail.Subtotal)}");
        lines.Add($"Desconto: {FormatCurrency(detail.DescontoTotal)}");
        lines.Add($"Taxa: {FormatCurrency(detail.TaxaTotal)}");
        lines.Add($"Liquido: {FormatCurrency(detail.TotalLiquido)}");

        if (!string.IsNullOrWhiteSpace(detail.Observacoes))
        {
            lines.Add(string.Empty);
            lines.Add($"Observacoes: {detail.Observacoes}");
        }

        if (detail.StatusVenda == SaleValues.SaleStatuses.Cancelada)
        {
            lines.Add(string.Empty);
            lines.Add($"Cancelada em: {detail.CanceladaEm:dd/MM/yyyy HH:mm}");
            lines.Add($"Motivo: {detail.MotivoCancelamento}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Atualiza metadados de alteracao das entidades mutaveis do modulo.
    /// </summary>
    private static void TouchEntity(AuditEntityBase entity, Guid usuarioId)
    {
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Normaliza um texto obrigatorio e gera erro amigavel quando vier vazio.
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
    /// Arredonda valores monetarios para duas casas antes de persistir.
    /// </summary>
    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Formata moeda para o recibo textual simples da venda.
    /// </summary>
    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    }

    /// <summary>
    /// Expone os status disponiveis para filtros do modulo.
    /// </summary>
    private static IReadOnlyList<SaleOptionResponse> BuildStatusOptions()
    {
        return
        [
            new(SaleValues.SaleStatuses.Concluida, "Concluida"),
            new(SaleValues.SaleStatuses.Cancelada, "Cancelada"),
        ];
    }

    // Modela o comprador com a relacao por loja necessaria para validar credito.
    private sealed record BuyerContext(Pessoa Pessoa, PessoaLoja Relacao);

    // Modela a peca pronta para compor itens de venda.
    private sealed record SalePieceInput(
        Peca Peca,
        string ProdutoNome,
        string Marca,
        string Tamanho,
        string Cor,
        string? FornecedorNome,
        PecaCondicaoComercial? CondicaoComercial,
        decimal PrecoBase,
        decimal PrecoEfetivoVenda,
        decimal PercentualDescontoAutomatico,
        bool DescontoAutomaticoAtivo);

    // Modela o pagamento ja normalizado e pronto para validacao.
    private sealed record NormalizedPaymentInput(
        string TipoPagamento,
        Guid? MeioPagamentoId,
        decimal Valor);

    // Modela o item de venda calculado com snapshot financeiro.
    private sealed record ComputedSaleItem(
        Guid PecaId,
        string CodigoInterno,
        int Quantidade,
        decimal PrecoTabelaUnitario,
        decimal DescontoUnitario,
        decimal PrecoFinalUnitario,
        decimal PercentualRepasseDinheiro,
        decimal PercentualRepasseCredito,
        decimal ValorRepassePrevisto,
        bool PermitePagamentoMisto);

    // Agrupa o item persistido com os nomes auxiliares exibidos no detalhe.
    private sealed record SaleAggregateItem(
        VendaItem Item,
        string CodigoInterno,
        string ProdutoNome,
        string Marca,
        string Tamanho,
        string Cor,
        string? FornecedorNome);

    // Agrupa o pagamento persistido com o nome do meio de pagamento.
    private sealed record SaleAggregatePayment(
        VendaPagamento Pagamento,
        string? MeioPagamentoNome);

    // Agrupa todas as informacoes de detalhe carregadas para uma venda.
    private sealed record SaleAggregate(
        Venda Venda,
        string VendedorNome,
        string? CompradorNome,
        IReadOnlyList<SaleAggregateItem> Itens,
        IReadOnlyList<SaleAggregatePayment> Pagamentos);
}
