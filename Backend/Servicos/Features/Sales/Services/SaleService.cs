using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.People;
using Renova.Services.Features.Pieces;
using Renova.Services.Features.Sales.Abstractions;
using Renova.Services.Features.Sales.Contracts;
using Renova.Services.Features.StockMovements.Abstractions;
using Renova.Services.Features.StockMovements.Contracts;

namespace Renova.Services.Features.Sales.Services;

// Implementa o modulo 09 com venda, cancelamento, estoque, financeiro e credito.
public sealed partial class SaleService : ISaleService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;
    private readonly IStockAvailabilityService _stockAvailabilityService;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria, contexto e regra de saldo.
    /// </summary>
    public SaleService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext,
        IStockAvailabilityService stockAvailabilityService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
        _stockAvailabilityService = stockAvailabilityService;
    }

    /// <summary>
    /// Carrega compradores, pecas disponiveis e meios de pagamento da loja ativa.
    /// </summary>
    public async Task<SalesWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureSalesViewContextAsync(cancellationToken);

        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var creditAccounts = await _dbContext.ContasCreditoLoja
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .ToListAsync(cancellationToken);

        var buyers = await (
                from pessoaLoja in _dbContext.PessoaLojas.AsNoTracking()
                join pessoa in _dbContext.Pessoas.AsNoTracking() on pessoaLoja.PessoaId equals pessoa.Id
                where pessoaLoja.LojaId == context.LojaId
                where pessoaLoja.EhCliente
                where pessoaLoja.StatusRelacao == PeopleStatusValues.StatusRelacao.Ativo
                orderby pessoa.Nome
                select new
                {
                    PessoaId = pessoa.Id,
                    pessoa.Nome,
                    pessoa.Documento,
                    pessoaLoja.AceitaCreditoLoja,
                })
            .ToListAsync(cancellationToken);

        var buyerOptions = buyers
            .Select(x =>
            {
                var account = creditAccounts.FirstOrDefault(account => account.PessoaId == x.PessoaId);
                return new SaleBuyerOptionResponse(
                    x.PessoaId,
                    x.Nome,
                    x.Documento,
                    x.AceitaCreditoLoja,
                    account?.SaldoAtual ?? 0m);
            })
            .ToArray();

        var pieces = await (
                from peca in _dbContext.Pecas.AsNoTracking()
                join produto in _dbContext.ProdutoNomes.AsNoTracking() on peca.ProdutoNomeId equals produto.Id
                join marca in _dbContext.Marcas.AsNoTracking() on peca.MarcaId equals marca.Id
                join tamanho in _dbContext.Tamanhos.AsNoTracking() on peca.TamanhoId equals tamanho.Id
                join cor in _dbContext.Cores.AsNoTracking() on peca.CorId equals cor.Id
                join condicao in _dbContext.PecaCondicoesComerciais.AsNoTracking() on peca.Id equals condicao.PecaId into conditionGroup
                from condicao in conditionGroup.DefaultIfEmpty()
                join fornecedor in _dbContext.Pessoas.AsNoTracking() on peca.FornecedorPessoaId equals fornecedor.Id into supplierGroup
                from fornecedor in supplierGroup.DefaultIfEmpty()
                where peca.LojaId == context.LojaId
                where peca.QuantidadeAtual > 0
                where peca.StatusPeca == PieceValues.PieceStatuses.Disponivel ||
                      peca.StatusPeca == PieceValues.PieceStatuses.Reservada
                orderby peca.CodigoInterno
                select new SalePieceOptionResponse(
                    peca.Id,
                    peca.CodigoInterno,
                    peca.CodigoBarras,
                    peca.TipoPeca,
                    peca.StatusPeca,
                    produto.Nome,
                    marca.Nome,
                    tamanho.Nome,
                    cor.Nome,
                    peca.FornecedorPessoaId,
                    fornecedor != null ? fornecedor.Nome : null,
                    peca.QuantidadeAtual,
                    peca.PrecoVendaAtual,
                    condicao != null ? condicao.PercentualRepasseDinheiro : 0m,
                    condicao != null ? condicao.PercentualRepasseCredito : 0m,
                    condicao != null && condicao.PermitePagamentoMisto))
            .ToListAsync(cancellationToken);

        var paymentMethods = await _dbContext.MeiosPagamento
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId && x.Ativo)
            .OrderBy(x => x.Nome)
            .Select(x => new SalePaymentMethodOptionResponse(
                x.Id,
                x.Nome,
                x.TipoMeioPagamento,
                SaleValues.GetPaymentMethodTypeLabel(x.TipoMeioPagamento),
                x.TaxaPercentual,
                x.PrazoRecebimentoDias))
            .ToListAsync(cancellationToken);

        var paymentTypeOptions = SaleValues.BuildPaymentTypeOptions()
            .Select(x => new SaleOptionResponse(x.Codigo, x.Nome))
            .ToArray();

        return new SalesWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            buyerOptions,
            pieces,
            paymentMethods,
            paymentTypeOptions,
            BuildStatusOptions());
    }

    /// <summary>
    /// Lista as vendas da loja ativa com filtros simples de consulta.
    /// </summary>
    public async Task<IReadOnlyList<SaleSummaryResponse>> ListarAsync(
        SaleListQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureSalesViewContextAsync(cancellationToken);

        var salesQuery =
            from venda in _dbContext.Vendas.AsNoTracking()
            join vendedor in _dbContext.Usuarios.AsNoTracking() on venda.VendedorUsuarioId equals vendedor.Id
            join comprador in _dbContext.Pessoas.AsNoTracking() on venda.CompradorPessoaId equals comprador.Id into buyerGroup
            from comprador in buyerGroup.DefaultIfEmpty()
            where venda.LojaId == context.LojaId
            select new
            {
                Venda = venda,
                VendedorNome = vendedor.Nome,
                CompradorNome = comprador != null ? comprador.Nome : null,
            };

        if (!string.IsNullOrWhiteSpace(query.StatusVenda))
        {
            var normalizedStatus = query.StatusVenda.Trim().ToLowerInvariant();
            salesQuery = salesQuery.Where(x => x.Venda.StatusVenda == normalizedStatus);
        }

        if (query.CompradorPessoaId.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.Venda.CompradorPessoaId == query.CompradorPessoaId.Value);
        }

        if (query.DataInicial.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.Venda.DataHoraVenda >= query.DataInicial.Value);
        }

        if (query.DataFinal.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.Venda.DataHoraVenda <= query.DataFinal.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            salesQuery = salesQuery.Where(x =>
                x.Venda.NumeroVenda.ToLower().Contains(term) ||
                x.VendedorNome.ToLower().Contains(term) ||
                (x.CompradorNome ?? string.Empty).ToLower().Contains(term));
        }

        var sales = await salesQuery
            .OrderByDescending(x => x.Venda.DataHoraVenda)
            .ThenByDescending(x => x.Venda.CriadoEm)
            .ToListAsync(cancellationToken);

        if (sales.Count == 0)
        {
            return [];
        }

        var saleIds = sales.Select(x => x.Venda.Id).ToArray();
        var itemCounts = await _dbContext.VendaItens
            .AsNoTracking()
            .Where(x => saleIds.Contains(x.VendaId))
            .GroupBy(x => x.VendaId)
            .Select(group => new { VendaId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var paymentCounts = await _dbContext.VendaPagamentos
            .AsNoTracking()
            .Where(x => saleIds.Contains(x.VendaId))
            .GroupBy(x => x.VendaId)
            .Select(group => new { VendaId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var itemCountMap = itemCounts.ToDictionary(x => x.VendaId, x => x.Count);
        var paymentCountMap = paymentCounts.ToDictionary(x => x.VendaId, x => x.Count);

        return sales
            .Select(x => new SaleSummaryResponse(
                x.Venda.Id,
                x.Venda.NumeroVenda,
                x.Venda.StatusVenda,
                x.Venda.DataHoraVenda,
                x.Venda.CompradorPessoaId,
                x.CompradorNome,
                x.Venda.VendedorUsuarioId,
                x.VendedorNome,
                x.Venda.Subtotal,
                x.Venda.DescontoTotal,
                x.Venda.TaxaTotal,
                x.Venda.TotalLiquido,
                itemCountMap.GetValueOrDefault(x.Venda.Id),
                paymentCountMap.GetValueOrDefault(x.Venda.Id)))
            .ToArray();
    }

    /// <summary>
    /// Carrega o detalhe completo da venda da loja ativa.
    /// </summary>
    public async Task<SaleDetailResponse> ObterDetalheAsync(Guid vendaId, CancellationToken cancellationToken = default)
    {
        var context = await EnsureSalesViewContextAsync(cancellationToken);
        var aggregate = await LoadSaleAggregateAsync(context.LojaId, vendaId, cancellationToken)
            ?? throw new InvalidOperationException("Venda nao encontrada na loja ativa.");

        return MapDetail(aggregate);
    }

    /// <summary>
    /// Registra a venda, baixa estoque e gera pagamentos, financeiro e obrigacoes.
    /// </summary>
    public async Task<SaleDetailResponse> CriarAsync(CreateSaleRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureSalesManageContextAsync(cancellationToken);
        ValidateCreateRequest(request);

        var normalizedPayments = request.Pagamentos
            .Select(payment => new NormalizedPaymentInput(
                SaleValues.NormalizePaymentType(payment.TipoPagamento),
                payment.MeioPagamentoId,
                RoundMoney(payment.Valor)))
            .ToArray();

        var saleRequests = request.Itens
            .Select(item => new StockSaleAvailabilityRequest(item.PecaId, item.Quantidade))
            .ToArray();

        await _stockAvailabilityService.EnsureSaleAvailabilityAsync(
            context.LojaId,
            saleRequests,
            cancellationToken);

        var itemIds = request.Itens.Select(x => x.PecaId).ToArray();
        if (itemIds.Distinct().Count() != itemIds.Length)
        {
            throw new InvalidOperationException("Nao repita a mesma peca na mesma venda.");
        }

        var pieces = await LoadSalePieceInputsAsync(context.LojaId, itemIds, cancellationToken);
        if (pieces.Count != itemIds.Length)
        {
            throw new InvalidOperationException("Informe apenas pecas validas da loja ativa.");
        }

        var financialPayments = normalizedPayments
            .Where(x => x.TipoPagamento == SaleValues.PaymentTypes.MeioPagamento)
            .ToArray();
        var creditPayments = normalizedPayments
            .Where(x => x.TipoPagamento == SaleValues.PaymentTypes.CreditoLoja)
            .ToArray();

        var paymentMethodIds = financialPayments
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

        foreach (var payment in financialPayments.Where(x => !x.MeioPagamentoId.HasValue))
        {
            _ = payment;
            throw new InvalidOperationException("Selecione o meio de pagamento para o valor financeiro da venda.");
        }

        if (creditPayments.Length > 0 && !request.CompradorPessoaId.HasValue)
        {
            throw new InvalidOperationException("Selecione o comprador para usar credito da loja.");
        }

        BuyerContext? buyerContext = null;
        ContaCreditoLoja? creditAccount = null;
        if (request.CompradorPessoaId.HasValue)
        {
            buyerContext = await LoadBuyerContextAsync(context.LojaId, request.CompradorPessoaId.Value, cancellationToken);
        }

        if (creditPayments.Length > 0)
        {
            buyerContext ??= await LoadBuyerContextAsync(context.LojaId, request.CompradorPessoaId!.Value, cancellationToken);
            if (!buyerContext.Relacao.AceitaCreditoLoja)
            {
                throw new InvalidOperationException("O comprador informado nao aceita credito da loja.");
            }

            creditAccount = await _dbContext.ContasCreditoLoja
                .FirstOrDefaultAsync(
                    x => x.LojaId == context.LojaId && x.PessoaId == buyerContext.Pessoa.Id,
                    cancellationToken)
                ?? throw new InvalidOperationException("O comprador nao possui conta de credito na loja ativa.");
        }

        var saleItems = BuildSaleItems(request.Itens, pieces, normalizedPayments);
        var subtotal = RoundMoney(saleItems.Sum(x => x.PrecoTabelaUnitario * x.Quantidade));
        var descontoTotal = RoundMoney(saleItems.Sum(x => x.DescontoUnitario * x.Quantidade));
        var totalVenda = RoundMoney(saleItems.Sum(x => x.PrecoFinalUnitario * x.Quantidade));
        var totalPagamentos = RoundMoney(normalizedPayments.Sum(x => x.Valor));

        if (Math.Abs(totalVenda - totalPagamentos) > 0.01m)
        {
            throw new InvalidOperationException("A soma dos pagamentos deve ser igual ao total da venda.");
        }

        var hasMixedPayment = financialPayments.Length > 0 && creditPayments.Length > 0;
        if (hasMixedPayment && saleItems.Any(x => !x.PermitePagamentoMisto))
        {
            throw new InvalidOperationException("Existe peca na venda que nao permite pagamento misto.");
        }

        var creditTotal = RoundMoney(creditPayments.Sum(x => x.Valor));
        if (creditAccount is not null && creditAccount.SaldoAtual < creditTotal)
        {
            throw new InvalidOperationException("O comprador nao possui saldo de credito suficiente para esta venda.");
        }

        var now = DateTimeOffset.UtcNow;
        var sale = new Venda
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            NumeroVenda = await GenerateSaleNumberAsync(context.LojaId, cancellationToken),
            StatusVenda = SaleValues.SaleStatuses.Concluida,
            DataHoraVenda = now,
            VendedorUsuarioId = context.UsuarioId,
            CompradorPessoaId = buyerContext?.Pessoa.Id,
            Subtotal = subtotal,
            DescontoTotal = descontoTotal,
            TaxaTotal = 0m,
            TotalLiquido = 0m,
            Observacoes = request.Observacoes.Trim(),
            CriadoPorUsuarioId = context.UsuarioId,
        };

        var pieceMap = pieces.ToDictionary(x => x.Peca.Id, x => x);
        var itemEntities = new List<VendaItem>();
        var stockMovements = new List<MovimentacaoEstoque>();
        var obligationEntities = new List<ObrigacaoFornecedor>();

        foreach (var saleItem in saleItems)
        {
            var pieceInput = pieceMap[saleItem.PecaId];
            var piece = pieceInput.Peca;
            var previousBalance = piece.QuantidadeAtual;
            piece.QuantidadeAtual -= saleItem.Quantidade;
            piece.StatusPeca = piece.QuantidadeAtual > 0
                ? PieceValues.PieceStatuses.Disponivel
                : PieceValues.PieceStatuses.Vendida;
            TouchEntity(piece, context.UsuarioId);

            var itemEntity = new VendaItem
            {
                Id = Guid.NewGuid(),
                VendaId = sale.Id,
                PecaId = piece.Id,
                Quantidade = saleItem.Quantidade,
                PrecoTabelaUnitario = saleItem.PrecoTabelaUnitario,
                DescontoUnitario = saleItem.DescontoUnitario,
                PrecoFinalUnitario = saleItem.PrecoFinalUnitario,
                TipoPecaSnapshot = piece.TipoPeca,
                FornecedorPessoaIdSnapshot = piece.FornecedorPessoaId,
                PercentualRepasseDinheiroSnapshot = saleItem.PercentualRepasseDinheiro,
                PercentualRepasseCreditoSnapshot = saleItem.PercentualRepasseCredito,
                ValorRepassePrevisto = saleItem.ValorRepassePrevisto,
                CriadoPorUsuarioId = context.UsuarioId,
            };

            itemEntities.Add(itemEntity);
            stockMovements.Add(new MovimentacaoEstoque
            {
                Id = Guid.NewGuid(),
                LojaId = context.LojaId,
                PecaId = piece.Id,
                TipoMovimentacao = PieceValues.StockMovementTypes.Venda,
                Quantidade = saleItem.Quantidade,
                SaldoAnterior = previousBalance,
                SaldoPosterior = piece.QuantidadeAtual,
                OrigemTipo = PieceValues.StockOrigins.Venda,
                OrigemId = sale.Id,
                Motivo = $"Venda {sale.NumeroVenda}",
                MovimentadoEm = now,
                MovimentadoPorUsuarioId = context.UsuarioId,
                CriadoPorUsuarioId = context.UsuarioId,
            });

            if (piece.TipoPeca == PieceValues.PieceTypes.Consignada &&
                piece.FornecedorPessoaId.HasValue &&
                saleItem.ValorRepassePrevisto > 0)
            {
                obligationEntities.Add(new ObrigacaoFornecedor
                {
                    Id = Guid.NewGuid(),
                    LojaId = context.LojaId,
                    PessoaId = piece.FornecedorPessoaId.Value,
                    VendaItemId = itemEntity.Id,
                    PecaId = piece.Id,
                    TipoObrigacao = SaleValues.SupplierObligationTypes.RepasseVendaConsignada,
                    DataGeracao = now,
                    DataVencimento = now,
                    ValorOriginal = saleItem.ValorRepassePrevisto,
                    ValorEmAberto = saleItem.ValorRepassePrevisto,
                    StatusObrigacao = SaleValues.SupplierObligationStatuses.Aberta,
                    Observacoes = $"Obrigacao gerada automaticamente pela venda {sale.NumeroVenda}.",
                    CriadoPorUsuarioId = context.UsuarioId,
                });
            }
        }

        var paymentEntities = new List<VendaPagamento>();
        var financialMovements = new List<MovimentacaoFinanceira>();
        var creditMovements = new List<MovimentacaoCreditoLoja>();
        var sequence = 1;

        foreach (var payment in normalizedPayments)
        {
            if (payment.TipoPagamento == SaleValues.PaymentTypes.MeioPagamento)
            {
                var paymentMethod = paymentMethods.First(x => x.Id == payment.MeioPagamentoId!.Value);
                var taxa = RoundMoney(payment.Valor * (paymentMethod.TaxaPercentual / 100m));
                var valorLiquido = RoundMoney(payment.Valor - taxa);

                var entity = new VendaPagamento
                {
                    Id = Guid.NewGuid(),
                    VendaId = sale.Id,
                    Sequencia = sequence++,
                    MeioPagamentoId = paymentMethod.Id,
                    TipoPagamento = payment.TipoPagamento,
                    ContaCreditoLojaId = null,
                    Valor = payment.Valor,
                    TaxaPercentualAplicada = paymentMethod.TaxaPercentual,
                    ValorLiquido = valorLiquido,
                    RecebidoEm = now,
                    CriadoPorUsuarioId = context.UsuarioId,
                };

                paymentEntities.Add(entity);
                financialMovements.Add(new MovimentacaoFinanceira
                {
                    Id = Guid.NewGuid(),
                    LojaId = context.LojaId,
                    TipoMovimentacao = SaleValues.FinancialMovementTypes.Venda,
                    Direcao = SaleValues.FinancialDirections.Entrada,
                    MeioPagamentoId = paymentMethod.Id,
                    VendaPagamentoId = entity.Id,
                    ValorBruto = entity.Valor,
                    Taxa = taxa,
                    ValorLiquido = valorLiquido,
                    Descricao = $"Recebimento da venda {sale.NumeroVenda}.",
                    CompetenciaEm = now.AddDays(paymentMethod.PrazoRecebimentoDias),
                    MovimentadoEm = now,
                    MovimentadoPorUsuarioId = context.UsuarioId,
                    CriadoPorUsuarioId = context.UsuarioId,
                });
            }
            else
            {
                if (creditAccount is null)
                {
                    throw new InvalidOperationException("Conta de credito nao encontrada para o comprador.");
                }

                var previousCreditBalance = creditAccount.SaldoAtual;
                creditAccount.SaldoAtual = RoundMoney(creditAccount.SaldoAtual - payment.Valor);
                TouchEntity(creditAccount, context.UsuarioId);

                var entity = new VendaPagamento
                {
                    Id = Guid.NewGuid(),
                    VendaId = sale.Id,
                    Sequencia = sequence++,
                    MeioPagamentoId = null,
                    TipoPagamento = payment.TipoPagamento,
                    ContaCreditoLojaId = creditAccount.Id,
                    Valor = payment.Valor,
                    TaxaPercentualAplicada = 0m,
                    ValorLiquido = payment.Valor,
                    RecebidoEm = now,
                    CriadoPorUsuarioId = context.UsuarioId,
                };

                paymentEntities.Add(entity);
                creditMovements.Add(new MovimentacaoCreditoLoja
                {
                    Id = Guid.NewGuid(),
                    ContaCreditoLojaId = creditAccount.Id,
                    TipoMovimentacao = SaleValues.CreditMovementTypes.DebitoVenda,
                    OrigemTipo = SaleValues.CreditOrigins.Venda,
                    OrigemId = sale.Id,
                    Valor = payment.Valor,
                    SaldoAnterior = previousCreditBalance,
                    SaldoPosterior = creditAccount.SaldoAtual,
                    Observacoes = $"Consumo de credito na venda {sale.NumeroVenda}.",
                    MovimentadoEm = now,
                    MovimentadoPorUsuarioId = context.UsuarioId,
                    CriadoPorUsuarioId = context.UsuarioId,
                });
            }
        }

        sale.TaxaTotal = RoundMoney(financialMovements.Sum(x => x.Taxa));
        sale.TotalLiquido = RoundMoney(paymentEntities.Sum(x => x.ValorLiquido));

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.Vendas.Add(sale);
        _dbContext.VendaItens.AddRange(itemEntities);
        _dbContext.VendaPagamentos.AddRange(paymentEntities);
        _dbContext.MovimentacoesEstoque.AddRange(stockMovements);
        _dbContext.MovimentacoesFinanceiras.AddRange(financialMovements);
        _dbContext.MovimentacoesCreditoLoja.AddRange(creditMovements);
        _dbContext.ObrigacoesFornecedor.AddRange(obligationEntities);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await RegisterCreateAuditsAsync(
            context.LojaId,
            sale,
            stockMovements,
            paymentEntities,
            financialMovements,
            creditMovements,
            obligationEntities,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return await ObterDetalheAsync(sale.Id, cancellationToken);
    }
}
