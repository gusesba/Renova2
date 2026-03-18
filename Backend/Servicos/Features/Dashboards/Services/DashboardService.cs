using Microsoft.EntityFrameworkCore;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Closings;
using Renova.Services.Features.Dashboards.Abstractions;
using Renova.Services.Features.Dashboards.Contracts;
using Renova.Services.Features.Financial;
using Renova.Services.Features.People;
using Renova.Services.Features.Pieces;
using Renova.Services.Features.Sales;
using Renova.Services.Features.SupplierPayments;

namespace Renova.Services.Features.Dashboards.Services;

// Implementa o modulo 14 com consultas consolidadas sobre as tabelas transacionais.
public sealed class DashboardService : IDashboardService
{
    private readonly RenovaDbContext _dbContext;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia e contexto autenticado.
    /// </summary>
    public DashboardService(
        RenovaDbContext dbContext,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega os filtros disponiveis da loja ativa para a tela de indicadores.
    /// </summary>
    public async Task<DashboardWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureDashboardViewContextAsync(cancellationToken);

        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var sellers = await (
                from membership in _dbContext.UsuarioLojas.AsNoTracking()
                join user in _dbContext.Usuarios.AsNoTracking() on membership.UsuarioId equals user.Id
                where membership.LojaId == context.LojaId
                where membership.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo
                where membership.DataFim == null || membership.DataFim >= DateTimeOffset.UtcNow
                orderby user.Nome
                select new DashboardFilterOptionResponse(user.Id, user.Nome, user.Email))
            .Distinct()
            .ToListAsync(cancellationToken);

        var suppliers = await (
                from relation in _dbContext.PessoaLojas.AsNoTracking()
                join person in _dbContext.Pessoas.AsNoTracking() on relation.PessoaId equals person.Id
                where relation.LojaId == context.LojaId
                where relation.StatusRelacao == PeopleStatusValues.StatusRelacao.Ativo
                where relation.EhFornecedor
                orderby person.Nome
                select new DashboardFilterOptionResponse(person.Id, person.Nome, person.Documento))
            .ToListAsync(cancellationToken);

        var brands = await _dbContext.Marcas
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderBy(x => x.Nome)
            .Select(x => new DashboardFilterOptionResponse(x.Id, x.Nome, null))
            .ToListAsync(cancellationToken);

        return new DashboardWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            sellers,
            suppliers,
            brands,
            DashboardValues.BuildPieceTypeOptions()
                .Select(x => new DashboardOptionResponse(x.Codigo, x.Nome))
                .ToArray());
    }

    /// <summary>
    /// Consolida os paineis de vendas, financeiro, consignacao, pendencias e indicadores.
    /// </summary>
    public async Task<DashboardOverviewResponse> ObterVisaoGeralAsync(
        DashboardQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureDashboardViewContextAsync(cancellationToken);
        var periodEnd = query.DataFinal.HasValue
            ? ToUtcEnd(query.DataFinal.Value)
            : DateTimeOffset.UtcNow;
        var periodStart = query.DataInicial.HasValue
            ? ToUtcStart(query.DataInicial.Value)
            : new DateTimeOffset(periodEnd.Year, periodEnd.Month, 1, 0, 0, 0, TimeSpan.Zero);

        if (periodEnd < periodStart)
        {
            throw new InvalidOperationException("O periodo final precisa ser maior ou igual ao periodo inicial.");
        }

        var normalizedPieceType = DashboardValues.NormalizePieceTypeFilter(query.TipoPeca);

        var filteredPieces = await (
                from piece in _dbContext.Pecas.AsNoTracking()
                join product in _dbContext.ProdutoNomes.AsNoTracking() on piece.ProdutoNomeId equals product.Id
                join brand in _dbContext.Marcas.AsNoTracking() on piece.MarcaId equals brand.Id
                join supplier in _dbContext.Pessoas.AsNoTracking() on piece.FornecedorPessoaId equals supplier.Id into supplierGroup
                from supplier in supplierGroup.DefaultIfEmpty()
                where piece.LojaId == context.LojaId
                where !query.FornecedorPessoaId.HasValue || piece.FornecedorPessoaId == query.FornecedorPessoaId.Value
                where !query.MarcaId.HasValue || piece.MarcaId == query.MarcaId.Value
                where normalizedPieceType == null || piece.TipoPeca == normalizedPieceType
                select new PieceProjection(
                    piece.Id,
                    piece.CodigoInterno,
                    piece.TipoPeca,
                    piece.StatusPeca,
                    piece.MarcaId,
                    brand.Nome,
                    piece.FornecedorPessoaId,
                    supplier != null ? supplier.Nome : null,
                    product.Nome,
                    piece.DataEntrada,
                    piece.QuantidadeAtual))
            .ToListAsync(cancellationToken);

        var soldItems = await (
                from saleItem in _dbContext.VendaItens.AsNoTracking()
                join sale in _dbContext.Vendas.AsNoTracking() on saleItem.VendaId equals sale.Id
                join piece in _dbContext.Pecas.AsNoTracking() on saleItem.PecaId equals piece.Id
                join seller in _dbContext.Usuarios.AsNoTracking() on sale.VendedorUsuarioId equals seller.Id
                where sale.LojaId == context.LojaId
                where sale.StatusVenda == SaleValues.SaleStatuses.Concluida
                where sale.DataHoraVenda >= periodStart && sale.DataHoraVenda <= periodEnd
                where !query.VendedorUsuarioId.HasValue || sale.VendedorUsuarioId == query.VendedorUsuarioId.Value
                where !query.FornecedorPessoaId.HasValue || saleItem.FornecedorPessoaIdSnapshot == query.FornecedorPessoaId.Value
                where !query.MarcaId.HasValue || piece.MarcaId == query.MarcaId.Value
                where normalizedPieceType == null || piece.TipoPeca == normalizedPieceType
                select new SoldItemProjection(
                    sale.Id,
                    sale.DataHoraVenda,
                    sale.VendedorUsuarioId,
                    seller.Nome,
                    saleItem.Quantidade,
                    RoundMoney(saleItem.PrecoFinalUnitario * saleItem.Quantidade),
                    piece.TipoPeca,
                    piece.MarcaId,
                    piece.FornecedorPessoaId))
            .ToListAsync(cancellationToken);

        var financialMovements = await _dbContext.MovimentacoesFinanceiras
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .Where(x => x.MovimentadoEm >= periodStart && x.MovimentadoEm <= periodEnd)
            .ToListAsync(cancellationToken);

        var salesSummary = BuildSalesResponse(soldItems);
        var financialSummary = BuildFinancialResponse(financialMovements);
        var consignmentSummary = await BuildConsignmentResponseAsync(filteredPieces, cancellationToken);
        var pendingSummary = await BuildPendingResponseAsync(
            context.LojaId,
            periodStart,
            periodEnd,
            query.FornecedorPessoaId,
            filteredPieces,
            cancellationToken);
        var indicatorsSummary = BuildIndicatorsResponse(filteredPieces, soldItems);

        return new DashboardOverviewResponse(
            periodStart,
            periodEnd,
            salesSummary,
            financialSummary,
            consignmentSummary,
            pendingSummary,
            indicatorsSummary);
    }

    /// <summary>
    /// Monta os agrupamentos do dashboard de vendas a partir dos itens vendidos.
    /// </summary>
    private static DashboardSalesResponse BuildSalesResponse(IReadOnlyList<SoldItemProjection> soldItems)
    {
        var totalVendido = RoundMoney(soldItems.Sum(x => x.TotalItem));
        var quantidadePecasVendidas = soldItems.Sum(x => x.Quantidade);
        var quantidadeVendas = soldItems.Select(x => x.VendaId).Distinct().Count();
        var ticketMedio = quantidadeVendas == 0 ? 0m : RoundMoney(totalVendido / quantidadeVendas);

        var byDay = soldItems
            .GroupBy(x => x.DataHoraVenda.UtcDateTime.Date)
            .OrderBy(x => x.Key)
            .Select(group => new DashboardBucketResponse(
                group.Key.ToString("yyyy-MM-dd"),
                group.Key.ToString("dd/MM"),
                group.Select(x => x.VendaId).Distinct().Count(),
                RoundMoney(group.Sum(x => x.TotalItem))))
            .ToArray();

        var byMonth = soldItems
            .GroupBy(x => new { x.DataHoraVenda.Year, x.DataHoraVenda.Month })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(group => new DashboardBucketResponse(
                $"{group.Key.Year:D4}-{group.Key.Month:D2}",
                $"{group.Key.Month:D2}/{group.Key.Year:D4}",
                group.Select(x => x.VendaId).Distinct().Count(),
                RoundMoney(group.Sum(x => x.TotalItem))))
            .ToArray();

        var byStore =
            new[]
            {
                new DashboardBucketResponse("loja_ativa", "Loja ativa", quantidadeVendas, totalVendido),
            };

        var bySeller = soldItems
            .GroupBy(x => new { x.VendedorUsuarioId, x.VendedorNome })
            .OrderByDescending(x => x.Sum(item => item.TotalItem))
            .Select(group => new DashboardBucketResponse(
                group.Key.VendedorUsuarioId.ToString(),
                group.Key.VendedorNome,
                group.Select(x => x.VendaId).Distinct().Count(),
                RoundMoney(group.Sum(x => x.TotalItem))))
            .ToArray();

        return new DashboardSalesResponse(
            quantidadeVendas,
            quantidadePecasVendidas,
            totalVendido,
            ticketMedio,
            byDay,
            byMonth,
            byStore,
            bySeller);
    }

    /// <summary>
    /// Consolida entradas, saidas e saldos do livro razao.
    /// </summary>
    private static DashboardFinancialResponse BuildFinancialResponse(
        IReadOnlyList<Domain.Models.MovimentacaoFinanceira> movements)
    {
        var entries = movements.Where(x => x.Direcao == FinancialValues.Directions.Entrada).ToArray();
        var exits = movements.Where(x => x.Direcao == FinancialValues.Directions.Saida).ToArray();

        var entradasBrutas = RoundMoney(entries.Sum(x => x.ValorBruto));
        var saidasBrutas = RoundMoney(exits.Sum(x => x.ValorBruto));
        var entradasLiquidas = RoundMoney(entries.Sum(x => x.ValorLiquido));
        var saidasLiquidas = RoundMoney(exits.Sum(x => x.ValorLiquido));

        return new DashboardFinancialResponse(
            entries.Length,
            exits.Length,
            entradasBrutas,
            saidasBrutas,
            RoundMoney(entradasBrutas - saidasBrutas),
            entradasLiquidas,
            saidasLiquidas,
            RoundMoney(entradasLiquidas - saidasLiquidas));
    }

    /// <summary>
    /// Monta as listas operacionais de consignacao proxima do vencimento e parada.
    /// </summary>
    private async Task<DashboardConsignmentResponse> BuildConsignmentResponseAsync(
        IReadOnlyList<PieceProjection> filteredPieces,
        CancellationToken cancellationToken)
    {
        var currentStatuses = new[]
        {
            PieceValues.PieceStatuses.Disponivel,
            PieceValues.PieceStatuses.Reservada,
            PieceValues.PieceStatuses.Inativa,
        };

        var pieceIds = filteredPieces.Select(x => x.PecaId).ToArray();
        var consignments = pieceIds.Length == 0
            ? new Dictionary<Guid, int>()
            : await (
                    from condition in _dbContext.PecaCondicoesComerciais.AsNoTracking()
                    where pieceIds.Contains(condition.PecaId)
                    select new
                    {
                        condition.PecaId,
                        condition.TempoMaximoExposicaoDias,
                    })
                .ToDictionaryAsync(x => x.PecaId, x => x.TempoMaximoExposicaoDias, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var nearDue = filteredPieces
            .Where(x => x.TipoPeca == PieceValues.PieceTypes.Consignada)
            .Where(x => currentStatuses.Contains(x.StatusPeca))
            .Where(x => consignments.ContainsKey(x.PecaId))
            .Select(piece =>
            {
                var limit = piece.DataEntrada.AddDays(consignments[piece.PecaId]);
                var daysToDue = (int)Math.Floor((limit - now).TotalDays);
                return new DashboardConsignmentItemResponse(
                    piece.PecaId,
                    piece.CodigoInterno,
                    piece.ProdutoNome,
                    piece.MarcaNome,
                    piece.FornecedorNome,
                    piece.DataEntrada,
                    (int)Math.Floor((now - piece.DataEntrada).TotalDays),
                    limit,
                    daysToDue);
            })
            .Where(x => x.DiasParaVencer <= DashboardValues.NearDueWindowDays)
            .OrderBy(x => x.DiasParaVencer)
            .Take(10)
            .ToArray();

        var stale = filteredPieces
            .Where(x => currentStatuses.Contains(x.StatusPeca))
            .Select(piece => new DashboardConsignmentItemResponse(
                piece.PecaId,
                piece.CodigoInterno,
                piece.ProdutoNome,
                piece.MarcaNome,
                piece.FornecedorNome,
                piece.DataEntrada,
                (int)Math.Floor((now - piece.DataEntrada).TotalDays),
                null,
                null))
            .Where(x => x.DiasEmEstoque >= DashboardValues.StaleStockThresholdDays)
            .OrderByDescending(x => x.DiasEmEstoque)
            .Take(10)
            .ToArray();

        return new DashboardConsignmentResponse(nearDue, stale);
    }

    /// <summary>
    /// Consolida valores em aberto e inconsistencias operacionais simples.
    /// </summary>
    private async Task<DashboardPendingResponse> BuildPendingResponseAsync(
        Guid lojaId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        Guid? fornecedorPessoaId,
        IReadOnlyList<PieceProjection> filteredPieces,
        CancellationToken cancellationToken)
    {
        var valorPagarFornecedores = await _dbContext.ObrigacoesFornecedor
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Where(x => !fornecedorPessoaId.HasValue || x.PessoaId == fornecedorPessoaId.Value)
            .Where(x => x.StatusObrigacao == SupplierPaymentValues.ObligationStatuses.Pendente ||
                        x.StatusObrigacao == SupplierPaymentValues.ObligationStatuses.Parcial)
            .SumAsync(x => (decimal?)x.ValorEmAberto, cancellationToken) ?? 0m;

        var valorPendenteRecebimento = await _dbContext.FechamentosPessoa
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Where(x => x.StatusFechamento != ClosingValues.Statuses.Liquidado)
            .Where(x => x.PeriodoFim >= periodStart && x.PeriodoInicio <= periodEnd)
            .Where(x => !fornecedorPessoaId.HasValue || x.PessoaId == fornecedorPessoaId.Value)
            .SumAsync(x => (decimal?)x.ValorAReceber, cancellationToken) ?? 0m;

        var inconsistencies = new List<DashboardPendingItemResponse>();

        inconsistencies.AddRange(filteredPieces
            .Where(x => x.StatusPeca == PieceValues.PieceStatuses.Vendida && x.QuantidadeAtual > 0)
            .Take(5)
            .Select(x => new DashboardPendingItemResponse(
                DashboardValues.InconsistencyTypes.PecaVendidaComSaldo,
                x.CodigoInterno,
                "Peca vendida ainda possui quantidade atual positiva.",
                x.QuantidadeAtual)));

        inconsistencies.AddRange(await _dbContext.ObrigacoesFornecedor
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Where(x => x.ValorEmAberto > x.ValorOriginal)
            .Take(5)
            .Select(x => new DashboardPendingItemResponse(
                DashboardValues.InconsistencyTypes.ObrigacaoAcimaOriginal,
                x.Id.ToString(),
                "Obrigacao com valor em aberto maior que o valor original.",
                x.ValorEmAberto))
            .ToListAsync(cancellationToken));

        inconsistencies.AddRange(await _dbContext.ContasCreditoLoja
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Where(x => x.SaldoAtual < 0m)
            .Take(5)
            .Select(x => new DashboardPendingItemResponse(
                DashboardValues.InconsistencyTypes.ContaCreditoNegativa,
                x.PessoaId.ToString(),
                "Conta de credito com saldo negativo.",
                x.SaldoAtual))
            .ToListAsync(cancellationToken));

        return new DashboardPendingResponse(
            RoundMoney(valorPagarFornecedores),
            RoundMoney(valorPendenteRecebimento),
            inconsistencies.Count,
            inconsistencies);
    }

    /// <summary>
    /// Monta os rankings por tipo, marca e fornecedor.
    /// </summary>
    private static DashboardIndicatorsResponse BuildIndicatorsResponse(
        IReadOnlyList<PieceProjection> filteredPieces,
        IReadOnlyList<SoldItemProjection> soldItems)
    {
        var byType = filteredPieces
            .GroupBy(x => x.TipoPeca)
            .Select(group => BuildIndicatorRow(
                group.Key,
                group.Key,
                group.Count(),
                group.Count(IsCurrentPiece),
                soldItems.Where(item => item.TipoPeca == group.Key).ToArray()))
            .OrderByDescending(x => x.ValorVendidoPeriodo)
            .ToArray();

        var byBrand = filteredPieces
            .GroupBy(x => new { x.MarcaId, x.MarcaNome })
            .Select(group => BuildIndicatorRow(
                group.Key.MarcaId.ToString(),
                group.Key.MarcaNome,
                group.Count(),
                group.Count(IsCurrentPiece),
                soldItems.Where(item => item.MarcaId == group.Key.MarcaId).ToArray()))
            .OrderByDescending(x => x.ValorVendidoPeriodo)
            .ToArray();

        var bySupplier = filteredPieces
            .GroupBy(x => new { x.FornecedorPessoaId, Nome = x.FornecedorNome ?? "Sem fornecedor" })
            .Select(group => BuildIndicatorRow(
                group.Key.FornecedorPessoaId?.ToString() ?? "sem-fornecedor",
                group.Key.Nome,
                group.Count(),
                group.Count(IsCurrentPiece),
                soldItems.Where(item => item.FornecedorPessoaId == group.Key.FornecedorPessoaId).ToArray()))
            .OrderByDescending(x => x.ValorVendidoPeriodo)
            .ToArray();

        return new DashboardIndicatorsResponse(byType, byBrand, bySupplier);
    }

    /// <summary>
    /// Exige loja ativa, vinculo valido e alguma permissao relacionada aos paineis.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureDashboardViewContextAsync(CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para continuar.");

        await EnsureStoreMembershipAsync(usuarioId, lojaId, cancellationToken);

        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [
                AccessPermissionCodes.VendasRegistrar,
                AccessPermissionCodes.VendasCancelar,
                AccessPermissionCodes.FinanceiroVisualizar,
                AccessPermissionCodes.FinanceiroConciliar,
                AccessPermissionCodes.PecasVisualizar,
                AccessPermissionCodes.PecasCadastrar,
                AccessPermissionCodes.PecasAjustar,
                AccessPermissionCodes.FechamentoGerar,
                AccessPermissionCodes.FechamentoConferir,
            ],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui acesso ao modulo de dashboards e indicadores.");
        }

        return (usuarioId, lojaId);
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
    /// Traduz o agrupamento em uma linha padrao dos indicadores.
    /// </summary>
    private static DashboardIndicatorRowResponse BuildIndicatorRow(
        string key,
        string name,
        int totalPieces,
        int currentPieces,
        IReadOnlyList<SoldItemProjection> soldItems)
    {
        return new DashboardIndicatorRowResponse(
            key,
            name,
            totalPieces,
            currentPieces,
            soldItems.Sum(x => x.Quantidade),
            RoundMoney(soldItems.Sum(x => x.TotalItem)));
    }

    /// <summary>
    /// Identifica se a peca ainda compoe o estoque operacional.
    /// </summary>
    private static bool IsCurrentPiece(PieceProjection piece)
    {
        return piece.StatusPeca == PieceValues.PieceStatuses.Disponivel ||
               piece.StatusPeca == PieceValues.PieceStatuses.Reservada ||
               piece.StatusPeca == PieceValues.PieceStatuses.Inativa;
    }

    /// <summary>
    /// Converte a data inicial do filtro para o inicio do dia em UTC.
    /// </summary>
    private static DateTimeOffset ToUtcStart(DateOnly value)
    {
        return new DateTimeOffset(value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    /// <summary>
    /// Converte a data final do filtro para o fim do dia em UTC.
    /// </summary>
    private static DateTimeOffset ToUtcEnd(DateOnly value)
    {
        return new DateTimeOffset(value.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddTicks(-1);
    }

    /// <summary>
    /// Arredonda valores monetarios para duas casas.
    /// </summary>
    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    // Projeta os campos da peca necessarios para os indicadores do modulo.
    private sealed record PieceProjection(
        Guid PecaId,
        string CodigoInterno,
        string TipoPeca,
        string StatusPeca,
        Guid MarcaId,
        string MarcaNome,
        Guid? FornecedorPessoaId,
        string? FornecedorNome,
        string ProdutoNome,
        DateTimeOffset DataEntrada,
        int QuantidadeAtual);

    // Projeta os itens vendidos usados pelos agrupamentos de vendas e indicadores.
    private sealed record SoldItemProjection(
        Guid VendaId,
        DateTimeOffset DataHoraVenda,
        Guid VendedorUsuarioId,
        string VendedorNome,
        int Quantidade,
        decimal TotalItem,
        string TipoPeca,
        Guid MarcaId,
        Guid? FornecedorPessoaId);
}
