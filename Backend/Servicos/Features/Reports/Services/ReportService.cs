using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.People;
using Renova.Services.Features.Pieces;
using Renova.Services.Features.Reports.Abstractions;
using Renova.Services.Features.Reports.Contracts;
using Renova.Services.Features.Sales;

namespace Renova.Services.Features.Reports.Services;

// Implementa o modulo 15 com consultas genericas, exportacao e filtros salvos.
public sealed class ReportService : IReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RenovaDbContext _dbContext;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia e contexto autenticado.
    /// </summary>
    public ReportService(
        RenovaDbContext dbContext,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega os filtros e listas auxiliares disponiveis no modulo.
    /// </summary>
    public async Task<ReportWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureReportContextAsync(null, cancellationToken);

        var activeStore = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var stores = await (
                from membership in _dbContext.UsuarioLojas.AsNoTracking()
                join store in _dbContext.Lojas.AsNoTracking() on membership.LojaId equals store.Id
                where membership.UsuarioId == context.UsuarioId
                where membership.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo
                where membership.DataFim == null || membership.DataFim >= DateTimeOffset.UtcNow
                orderby store.NomeFantasia
                select new ReportFilterOptionResponse(store.Id, store.NomeFantasia, store.Documento))
            .Distinct()
            .ToListAsync(cancellationToken);

        var storeIds = stores.Select(x => x.Id).ToArray();

        var suppliers = await (
                from relation in _dbContext.PessoaLojas.AsNoTracking()
                join person in _dbContext.Pessoas.AsNoTracking() on relation.PessoaId equals person.Id
                where storeIds.Contains(relation.LojaId)
                where relation.StatusRelacao == PeopleStatusValues.StatusRelacao.Ativo
                where relation.EhFornecedor
                orderby person.Nome
                select new ReportFilterOptionResponse(person.Id, person.Nome, person.Documento))
            .Distinct()
            .ToListAsync(cancellationToken);

        var financialPeople = await (
                from relation in _dbContext.PessoaLojas.AsNoTracking()
                join person in _dbContext.Pessoas.AsNoTracking() on relation.PessoaId equals person.Id
                where storeIds.Contains(relation.LojaId)
                where relation.StatusRelacao == PeopleStatusValues.StatusRelacao.Ativo
                where relation.EhCliente || relation.EhFornecedor
                orderby person.Nome
                select new ReportFilterOptionResponse(person.Id, person.Nome, person.Documento))
            .Distinct()
            .ToListAsync(cancellationToken);

        var brands = await _dbContext.Marcas
            .AsNoTracking()
            .Where(x => storeIds.Contains(x.LojaId))
            .OrderBy(x => x.Nome)
            .Select(x => new ReportFilterOptionResponse(x.Id, x.Nome, null))
            .Distinct()
            .ToListAsync(cancellationToken);

        var sellers = await (
                from membership in _dbContext.UsuarioLojas.AsNoTracking()
                join user in _dbContext.Usuarios.AsNoTracking() on membership.UsuarioId equals user.Id
                where storeIds.Contains(membership.LojaId)
                where membership.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo
                where membership.DataFim == null || membership.DataFim >= DateTimeOffset.UtcNow
                orderby user.Nome
                select new ReportFilterOptionResponse(user.Id, user.Nome, user.Email))
            .Distinct()
            .ToListAsync(cancellationToken);

        var savedFilters = await _dbContext.RelatorioFiltrosSalvos
            .AsNoTracking()
            .Where(x => x.UsuarioId == context.UsuarioId && x.Ativo)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        return new ReportWorkspaceResponse(
            activeStore.Id,
            activeStore.NomeFantasia,
            stores,
            suppliers,
            financialPeople,
            brands,
            sellers,
            BuildPieceStatusOptions(),
            BuildStockMovementReasonOptions(),
            BuildReportTypeOptions(),
            savedFilters.Select(MapSavedFilter).ToArray());
    }

    /// <summary>
    /// Executa o relatorio solicitado e retorna o grid consolidado.
    /// </summary>
    public async Task<ReportResultResponse> ExecutarAsync(
        ReportQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = ReportValues.NormalizeReportType(query.TipoRelatorio);
        var context = await EnsureReportContextAsync(query.LojaId, cancellationToken);
        var normalizedQuery = NormalizeQuery(normalizedType, context.LojaId, query);

        return normalizedType switch
        {
            ReportValues.ReportTypes.EstoqueAtual => await BuildInventoryReportAsync(normalizedQuery, cancellationToken),
            ReportValues.ReportTypes.PecasVendidas => await BuildSoldPiecesReportAsync(normalizedQuery, cancellationToken),
            ReportValues.ReportTypes.Financeiro => await BuildFinancialReportAsync(normalizedQuery, cancellationToken),
            ReportValues.ReportTypes.BaixasEstoque => await BuildStockDisposalReportAsync(normalizedQuery, cancellationToken),
            _ => throw new InvalidOperationException("Tipo de relatorio nao suportado."),
        };
    }

    /// <summary>
    /// Gera o arquivo exportavel em HTML imprimivel ou CSV compativel com Excel.
    /// </summary>
    public async Task<ReportExportFileResponse> ExportarAsync(
        string format,
        ReportQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var normalizedFormat = ReportValues.NormalizeExportFormat(format);
        var result = await ExecutarAsync(query, cancellationToken);

        return normalizedFormat == ReportValues.ExportFormats.Pdf
            ? new ReportExportFileResponse(
                BuildFileName(result, "html"),
                "text/html; charset=utf-8",
                Encoding.UTF8.GetBytes(BuildPrintableHtml(result)))
            : new ReportExportFileResponse(
                BuildFileName(result, "csv"),
                "text/csv; charset=utf-8",
                Encoding.UTF8.GetBytes(BuildCsv(result)));
    }

    /// <summary>
    /// Persiste um filtro frequente para reaproveitamento posterior.
    /// </summary>
    public async Task<SavedReportFilterResponse> SalvarFiltroAsync(
        SaveReportFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureReportContextAsync(request.Filtros.LojaId, cancellationToken);
        var normalizedType = ReportValues.NormalizeReportType(request.Filtros.TipoRelatorio);
        var filterName = request.Nome.Trim();

        if (string.IsNullOrWhiteSpace(filterName))
        {
            throw new InvalidOperationException("Informe o nome do filtro salvo.");
        }

        var normalizedQuery = NormalizeQuery(normalizedType, context.LojaId, request.Filtros);

        var existing = await _dbContext.RelatorioFiltrosSalvos
            .FirstOrDefaultAsync(
                x => x.UsuarioId == context.UsuarioId &&
                     x.LojaId == context.LojaId &&
                     x.TipoRelatorio == normalizedType &&
                     x.Nome.ToLower() == filterName.ToLower(),
                cancellationToken);

        if (existing is null)
        {
            existing = new RelatorioFiltroSalvo
            {
                LojaId = context.LojaId,
                UsuarioId = context.UsuarioId,
                Nome = filterName,
                TipoRelatorio = normalizedType,
                FiltrosJson = JsonSerializer.Serialize(normalizedQuery, JsonOptions),
                Ativo = true,
                CriadoEm = DateTimeOffset.UtcNow,
                CriadoPorUsuarioId = context.UsuarioId,
            };

            _dbContext.RelatorioFiltrosSalvos.Add(existing);
        }
        else
        {
            existing.FiltrosJson = JsonSerializer.Serialize(normalizedQuery, JsonOptions);
            existing.Ativo = true;
            existing.AtualizadoEm = DateTimeOffset.UtcNow;
            existing.AtualizadoPorUsuarioId = context.UsuarioId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapSavedFilter(existing);
    }

    /// <summary>
    /// Inativa um filtro salvo do usuario autenticado.
    /// </summary>
    public async Task RemoverFiltroAsync(Guid filtroId, CancellationToken cancellationToken = default)
    {
        var context = await EnsureReportContextAsync(null, cancellationToken);

        var filter = await _dbContext.RelatorioFiltrosSalvos
            .FirstOrDefaultAsync(
                x => x.Id == filtroId &&
                     x.UsuarioId == context.UsuarioId &&
                     x.Ativo,
                cancellationToken)
            ?? throw new InvalidOperationException("Filtro salvo nao encontrado.");

        filter.Ativo = false;
        filter.InativadoEm = DateTimeOffset.UtcNow;
        filter.AtualizadoEm = DateTimeOffset.UtcNow;
        filter.AtualizadoPorUsuarioId = context.UsuarioId;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Monta o relatorio de estoque atual da loja selecionada.
    /// </summary>
    private async Task<ReportResultResponse> BuildInventoryReportAsync(
        ReportQueryRequest query,
        CancellationToken cancellationToken)
    {
        var items = await (
                from piece in _dbContext.Pecas.AsNoTracking()
                join product in _dbContext.ProdutoNomes.AsNoTracking() on piece.ProdutoNomeId equals product.Id
                join brand in _dbContext.Marcas.AsNoTracking() on piece.MarcaId equals brand.Id
                join supplier in _dbContext.Pessoas.AsNoTracking() on piece.FornecedorPessoaId equals supplier.Id into supplierGroup
                from supplier in supplierGroup.DefaultIfEmpty()
                where piece.LojaId == query.LojaId
                where !query.FornecedorPessoaId.HasValue || piece.FornecedorPessoaId == query.FornecedorPessoaId.Value
                where !query.MarcaId.HasValue || piece.MarcaId == query.MarcaId.Value
                where string.IsNullOrWhiteSpace(query.StatusPeca) || piece.StatusPeca == query.StatusPeca
                where string.IsNullOrWhiteSpace(query.Search) ||
                      piece.CodigoInterno.ToLower().Contains(query.Search!) ||
                      piece.CodigoBarras.ToLower().Contains(query.Search!) ||
                      product.Nome.ToLower().Contains(query.Search!) ||
                      brand.Nome.ToLower().Contains(query.Search!)
                orderby piece.CodigoInterno
                select new
                {
                    piece.Id,
                    piece.CodigoInterno,
                    piece.CodigoBarras,
                    ProdutoNome = product.Nome,
                    MarcaNome = brand.Nome,
                    FornecedorNome = supplier != null ? supplier.Nome : "Sem fornecedor",
                    piece.TipoPeca,
                    piece.StatusPeca,
                    piece.QuantidadeAtual,
                    piece.PrecoVendaAtual,
                    piece.DataEntrada,
                })
            .ToListAsync(cancellationToken);

        return new ReportResultResponse(
            query.TipoRelatorio,
            "Relatorio de estoque atual",
            "Visao do estoque da loja com filtros por status, marca e fornecedor.",
            [
                new("Pecas", items.Count.ToString()),
                new("Quantidade em estoque", items.Sum(x => x.QuantidadeAtual).ToString()),
                new("Valor potencial", FormatCurrency(items.Sum(x => x.PrecoVendaAtual * x.QuantidadeAtual))),
            ],
            [
                new("codigoInterno", "Codigo"),
                new("codigoBarras", "Codigo de barras"),
                new("produto", "Produto"),
                new("marca", "Marca"),
                new("fornecedor", "Fornecedor"),
                new("tipoPeca", "Tipo"),
                new("statusPeca", "Status"),
                new("quantidadeAtual", "Qtd."),
                new("precoVendaAtual", "Preco venda"),
                new("dataEntrada", "Entrada"),
            ],
            items.Select(item => new ReportRowResponse(
                item.Id.ToString(),
                [
                    new("codigoInterno", item.CodigoInterno),
                    new("codigoBarras", item.CodigoBarras),
                    new("produto", item.ProdutoNome),
                    new("marca", item.MarcaNome),
                    new("fornecedor", item.FornecedorNome),
                    new("tipoPeca", item.TipoPeca),
                    new("statusPeca", item.StatusPeca),
                    new("quantidadeAtual", item.QuantidadeAtual.ToString()),
                    new("precoVendaAtual", FormatCurrency(item.PrecoVendaAtual)),
                    new("dataEntrada", FormatDate(item.DataEntrada)),
                ]))
                .ToArray(),
            items.Count);
    }

    /// <summary>
    /// Monta o relatorio de pecas vendidas por periodo, fornecedor e vendedor.
    /// </summary>
    private async Task<ReportResultResponse> BuildSoldPiecesReportAsync(
        ReportQueryRequest query,
        CancellationToken cancellationToken)
    {
        var start = query.DataInicial.HasValue ? ToUtcStart(query.DataInicial.Value) : DateTimeOffset.MinValue;
        var end = query.DataFinal.HasValue ? ToUtcEnd(query.DataFinal.Value) : DateTimeOffset.UtcNow;

        var items = await (
                from saleItem in _dbContext.VendaItens.AsNoTracking()
                join sale in _dbContext.Vendas.AsNoTracking() on saleItem.VendaId equals sale.Id
                join piece in _dbContext.Pecas.AsNoTracking() on saleItem.PecaId equals piece.Id
                join product in _dbContext.ProdutoNomes.AsNoTracking() on piece.ProdutoNomeId equals product.Id
                join brand in _dbContext.Marcas.AsNoTracking() on piece.MarcaId equals brand.Id
                join seller in _dbContext.Usuarios.AsNoTracking() on sale.VendedorUsuarioId equals seller.Id
                join buyer in _dbContext.Pessoas.AsNoTracking() on sale.CompradorPessoaId equals buyer.Id into buyerGroup
                from buyer in buyerGroup.DefaultIfEmpty()
                join supplier in _dbContext.Pessoas.AsNoTracking() on saleItem.FornecedorPessoaIdSnapshot equals supplier.Id into supplierGroup
                from supplier in supplierGroup.DefaultIfEmpty()
                where sale.LojaId == query.LojaId
                where sale.StatusVenda == SaleValues.SaleStatuses.Concluida
                where sale.DataHoraVenda >= start && sale.DataHoraVenda <= end
                where !query.FornecedorPessoaId.HasValue || saleItem.FornecedorPessoaIdSnapshot == query.FornecedorPessoaId.Value
                where !query.VendedorUsuarioId.HasValue || sale.VendedorUsuarioId == query.VendedorUsuarioId.Value
                where !query.MarcaId.HasValue || piece.MarcaId == query.MarcaId.Value
                where string.IsNullOrWhiteSpace(query.Search) ||
                      sale.NumeroVenda.ToLower().Contains(query.Search!) ||
                      piece.CodigoInterno.ToLower().Contains(query.Search!) ||
                      product.Nome.ToLower().Contains(query.Search!)
                orderby sale.DataHoraVenda descending, sale.NumeroVenda
                select new
                {
                    saleItem.Id,
                    sale.NumeroVenda,
                    sale.DataHoraVenda,
                    piece.CodigoInterno,
                    ProdutoNome = product.Nome,
                    MarcaNome = brand.Nome,
                    FornecedorNome = supplier != null ? supplier.Nome : "Sem fornecedor",
                    VendedorNome = seller.Nome,
                    CompradorNome = buyer != null ? buyer.Nome : "Sem comprador",
                    saleItem.Quantidade,
                    saleItem.PrecoTabelaUnitario,
                    saleItem.DescontoUnitario,
                    saleItem.PrecoFinalUnitario,
                })
            .ToListAsync(cancellationToken);

        return new ReportResultResponse(
            query.TipoRelatorio,
            "Relatorio de pecas vendidas",
            "Consulta vendas concluidas por periodo, fornecedor e vendedor.",
            [
                new("Itens vendidos", items.Sum(x => x.Quantidade).ToString()),
                new("Valor bruto", FormatCurrency(items.Sum(x => x.PrecoTabelaUnitario * x.Quantidade))),
                new("Desconto", FormatCurrency(items.Sum(x => x.DescontoUnitario * x.Quantidade))),
                new("Valor final", FormatCurrency(items.Sum(x => x.PrecoFinalUnitario * x.Quantidade))),
            ],
            [
                new("dataVenda", "Data venda"),
                new("numeroVenda", "Venda"),
                new("codigoInterno", "Codigo"),
                new("produto", "Produto"),
                new("marca", "Marca"),
                new("fornecedor", "Fornecedor"),
                new("vendedor", "Vendedor"),
                new("comprador", "Comprador"),
                new("quantidade", "Qtd."),
                new("precoTabela", "Preco tabela"),
                new("desconto", "Desconto"),
                new("precoFinal", "Preco final"),
            ],
            items.Select(item => new ReportRowResponse(
                item.Id.ToString(),
                [
                    new("dataVenda", FormatDate(item.DataHoraVenda)),
                    new("numeroVenda", item.NumeroVenda),
                    new("codigoInterno", item.CodigoInterno),
                    new("produto", item.ProdutoNome),
                    new("marca", item.MarcaNome),
                    new("fornecedor", item.FornecedorNome),
                    new("vendedor", item.VendedorNome),
                    new("comprador", item.CompradorNome),
                    new("quantidade", item.Quantidade.ToString()),
                    new("precoTabela", FormatCurrency(item.PrecoTabelaUnitario)),
                    new("desconto", FormatCurrency(item.DescontoUnitario)),
                    new("precoFinal", FormatCurrency(item.PrecoFinalUnitario)),
                ]))
                .ToArray(),
            items.Count);
    }

    /// <summary>
    /// Monta o relatorio financeiro por pessoa, periodo e loja.
    /// </summary>
    private async Task<ReportResultResponse> BuildFinancialReportAsync(
        ReportQueryRequest query,
        CancellationToken cancellationToken)
    {
        var start = query.DataInicial.HasValue ? ToUtcStart(query.DataInicial.Value) : DateTimeOffset.MinValue;
        var end = query.DataFinal.HasValue ? ToUtcEnd(query.DataFinal.Value) : DateTimeOffset.UtcNow;

        var entries = await (
                from movement in _dbContext.MovimentacoesFinanceiras.AsNoTracking()
                join paymentMethod in _dbContext.MeiosPagamento.AsNoTracking() on movement.MeioPagamentoId equals paymentMethod.Id into paymentMethodGroup
                from paymentMethod in paymentMethodGroup.DefaultIfEmpty()
                join salePayment in _dbContext.VendaPagamentos.AsNoTracking() on movement.VendaPagamentoId equals salePayment.Id into salePaymentGroup
                from salePayment in salePaymentGroup.DefaultIfEmpty()
                join sale in _dbContext.Vendas.AsNoTracking() on salePayment.VendaId equals sale.Id into saleGroup
                from sale in saleGroup.DefaultIfEmpty()
                join salePerson in _dbContext.Pessoas.AsNoTracking() on sale.CompradorPessoaId equals salePerson.Id into salePersonGroup
                from salePerson in salePersonGroup.DefaultIfEmpty()
                join liquidation in _dbContext.LiquidacoesObrigacaoFornecedor.AsNoTracking() on movement.LiquidacaoObrigacaoFornecedorId equals liquidation.Id into liquidationGroup
                from liquidation in liquidationGroup.DefaultIfEmpty()
                join obligation in _dbContext.ObrigacoesFornecedor.AsNoTracking() on liquidation.ObrigacaoFornecedorId equals obligation.Id into obligationGroup
                from obligation in obligationGroup.DefaultIfEmpty()
                join supplier in _dbContext.Pessoas.AsNoTracking() on obligation.PessoaId equals supplier.Id into supplierGroup
                from supplier in supplierGroup.DefaultIfEmpty()
                where movement.LojaId == query.LojaId
                where movement.MovimentadoEm >= start && movement.MovimentadoEm <= end
                where !query.PessoaId.HasValue ||
                      (salePerson != null && salePerson.Id == query.PessoaId.Value) ||
                      (supplier != null && supplier.Id == query.PessoaId.Value)
                where string.IsNullOrWhiteSpace(query.Search) ||
                      movement.Descricao.ToLower().Contains(query.Search!) ||
                      (sale != null && sale.NumeroVenda.ToLower().Contains(query.Search!)) ||
                      (salePerson != null && salePerson.Nome.ToLower().Contains(query.Search!)) ||
                      (supplier != null && supplier.Nome.ToLower().Contains(query.Search!))
                orderby movement.MovimentadoEm descending
                select new
                {
                    movement.Id,
                    movement.MovimentadoEm,
                    movement.TipoMovimentacao,
                    movement.Direcao,
                    movement.Descricao,
                    MeioPagamentoNome = paymentMethod != null ? paymentMethod.Nome : "Sem meio",
                    PessoaNome = salePerson != null
                        ? salePerson.Nome
                        : supplier != null
                            ? supplier.Nome
                            : "Sem pessoa vinculada",
                    PessoaDocumento = salePerson != null
                        ? salePerson.Documento
                        : supplier != null
                            ? supplier.Documento
                            : string.Empty,
                    movement.ValorBruto,
                    movement.Taxa,
                    movement.ValorLiquido,
                })
            .ToListAsync(cancellationToken);

        return new ReportResultResponse(
            query.TipoRelatorio,
            "Relatorio financeiro",
            "Consulta movimentos financeiros por pessoa, periodo e loja.",
            [
                new("Lancamentos", entries.Count.ToString()),
                new("Entradas", FormatCurrency(entries.Where(x => x.Direcao == "entrada").Sum(x => x.ValorLiquido))),
                new("Saidas", FormatCurrency(entries.Where(x => x.Direcao == "saida").Sum(x => x.ValorLiquido))),
                new("Saldo liquido", FormatCurrency(entries.Sum(x => x.Direcao == "entrada" ? x.ValorLiquido : -x.ValorLiquido))),
            ],
            [
                new("movimentadoEm", "Movimentado em"),
                new("tipoMovimentacao", "Tipo"),
                new("direcao", "Direcao"),
                new("descricao", "Descricao"),
                new("pessoa", "Pessoa"),
                new("documento", "Documento"),
                new("meioPagamento", "Meio"),
                new("valorBruto", "Valor bruto"),
                new("taxa", "Taxa"),
                new("valorLiquido", "Valor liquido"),
            ],
            entries.Select(item => new ReportRowResponse(
                item.Id.ToString(),
                [
                    new("movimentadoEm", FormatDate(item.MovimentadoEm)),
                    new("tipoMovimentacao", item.TipoMovimentacao),
                    new("direcao", item.Direcao),
                    new("descricao", item.Descricao),
                    new("pessoa", item.PessoaNome),
                    new("documento", item.PessoaDocumento),
                    new("meioPagamento", item.MeioPagamentoNome),
                    new("valorBruto", FormatCurrency(item.ValorBruto)),
                    new("taxa", FormatCurrency(item.Taxa)),
                    new("valorLiquido", FormatCurrency(item.ValorLiquido)),
                ]))
                .ToArray(),
            entries.Count);
    }

    /// <summary>
    /// Monta o relatorio de devolucoes, doacoes, perdas e descartes.
    /// </summary>
    private async Task<ReportResultResponse> BuildStockDisposalReportAsync(
        ReportQueryRequest query,
        CancellationToken cancellationToken)
    {
        var start = query.DataInicial.HasValue ? ToUtcStart(query.DataInicial.Value) : DateTimeOffset.MinValue;
        var end = query.DataFinal.HasValue ? ToUtcEnd(query.DataFinal.Value) : DateTimeOffset.UtcNow;

        var entries = await (
                from movement in _dbContext.MovimentacoesEstoque.AsNoTracking()
                join piece in _dbContext.Pecas.AsNoTracking() on movement.PecaId equals piece.Id
                join product in _dbContext.ProdutoNomes.AsNoTracking() on piece.ProdutoNomeId equals product.Id
                join brand in _dbContext.Marcas.AsNoTracking() on piece.MarcaId equals brand.Id
                join supplier in _dbContext.Pessoas.AsNoTracking() on piece.FornecedorPessoaId equals supplier.Id into supplierGroup
                from supplier in supplierGroup.DefaultIfEmpty()
                join user in _dbContext.Usuarios.AsNoTracking() on movement.MovimentadoPorUsuarioId equals user.Id
                where movement.LojaId == query.LojaId
                where movement.MovimentadoEm >= start && movement.MovimentadoEm <= end
                where query.MotivoMovimentacao == null || movement.TipoMovimentacao == query.MotivoMovimentacao
                where movement.TipoMovimentacao == PieceValues.StockMovementTypes.Devolucao ||
                      movement.TipoMovimentacao == PieceValues.StockMovementTypes.Doacao ||
                      movement.TipoMovimentacao == PieceValues.StockMovementTypes.Perda ||
                      movement.TipoMovimentacao == PieceValues.StockMovementTypes.Descarte
                where string.IsNullOrWhiteSpace(query.Search) ||
                      piece.CodigoInterno.ToLower().Contains(query.Search!) ||
                      product.Nome.ToLower().Contains(query.Search!) ||
                      movement.Motivo.ToLower().Contains(query.Search!)
                orderby movement.MovimentadoEm descending
                select new
                {
                    movement.Id,
                    movement.MovimentadoEm,
                    movement.TipoMovimentacao,
                    movement.Motivo,
                    piece.CodigoInterno,
                    ProdutoNome = product.Nome,
                    MarcaNome = brand.Nome,
                    FornecedorNome = supplier != null ? supplier.Nome : "Sem fornecedor",
                    ResponsavelNome = user.Nome,
                    movement.Quantidade,
                })
            .ToListAsync(cancellationToken);

        return new ReportResultResponse(
            query.TipoRelatorio,
            "Relatorio de baixas de estoque",
            "Consulta pecas devolvidas, doadas, perdidas e descartadas por periodo e motivo.",
            [
                new("Movimentacoes", entries.Count.ToString()),
                new("Quantidade baixada", entries.Sum(x => Math.Abs(x.Quantidade)).ToString()),
                new("Devolucoes", entries.Count(x => x.TipoMovimentacao == PieceValues.StockMovementTypes.Devolucao).ToString()),
                new("Outras baixas", entries.Count(x => x.TipoMovimentacao != PieceValues.StockMovementTypes.Devolucao).ToString()),
            ],
            [
                new("movimentadoEm", "Data"),
                new("tipoMovimentacao", "Tipo"),
                new("motivo", "Motivo"),
                new("codigoInterno", "Codigo"),
                new("produto", "Produto"),
                new("marca", "Marca"),
                new("fornecedor", "Fornecedor"),
                new("responsavel", "Responsavel"),
                new("quantidade", "Qtd."),
            ],
            entries.Select(item => new ReportRowResponse(
                item.Id.ToString(),
                [
                    new("movimentadoEm", FormatDate(item.MovimentadoEm)),
                    new("tipoMovimentacao", item.TipoMovimentacao),
                    new("motivo", item.Motivo),
                    new("codigoInterno", item.CodigoInterno),
                    new("produto", item.ProdutoNome),
                    new("marca", item.MarcaNome),
                    new("fornecedor", item.FornecedorNome),
                    new("responsavel", item.ResponsavelNome),
                    new("quantidade", Math.Abs(item.Quantidade).ToString()),
                ]))
                .ToArray(),
            entries.Count);
    }

    /// <summary>
    /// Exige autenticacao, loja selecionada valida e permissao de exportar relatorios.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureReportContextAsync(
        Guid? requestedStoreId,
        CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = requestedStoreId ?? _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja para continuar.");

        await EnsureStoreMembershipAsync(usuarioId, lojaId, cancellationToken);

        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.RelatoriosExportar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui acesso ao modulo de relatorios e exportacoes.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Garante que o usuario possui vinculo valido com a loja consultada.
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
            throw new InvalidOperationException("Voce nao possui acesso a loja informada.");
        }
    }

    /// <summary>
    /// Verifica se o usuario possui alguma permissao na loja consultada.
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
    /// Normaliza os filtros do request para reaproveitamento interno e persistencia.
    /// </summary>
    private static ReportQueryRequest NormalizeQuery(string reportType, Guid lojaId, ReportQueryRequest query)
    {
        if (query.DataInicial.HasValue && query.DataFinal.HasValue && query.DataFinal < query.DataInicial)
        {
            throw new InvalidOperationException("A data final precisa ser maior ou igual a data inicial.");
        }

        var normalizedStatus = string.IsNullOrWhiteSpace(query.StatusPeca)
            ? null
            : PieceValues.NormalizePieceStatus(query.StatusPeca);
        var normalizedReason = string.IsNullOrWhiteSpace(query.MotivoMovimentacao)
            ? null
            : NormalizeStockMovementReason(query.MotivoMovimentacao);

        return query with
        {
            TipoRelatorio = reportType,
            LojaId = lojaId,
            StatusPeca = normalizedStatus,
            MotivoMovimentacao = normalizedReason,
            Search = string.IsNullOrWhiteSpace(query.Search)
                ? null
                : query.Search.Trim().ToLowerInvariant(),
        };
    }

    /// <summary>
    /// Traduz o filtro salvo persistido para o contrato de retorno.
    /// </summary>
    private static SavedReportFilterResponse MapSavedFilter(RelatorioFiltroSalvo entity)
    {
        var filters = JsonSerializer.Deserialize<ReportQueryRequest>(entity.FiltrosJson, JsonOptions)
            ?? new ReportQueryRequest(entity.TipoRelatorio, entity.LojaId, null, null, null, null, null, null, null, null, null);

        return new SavedReportFilterResponse(
            entity.Id,
            entity.Nome,
            entity.TipoRelatorio,
            filters,
            entity.CriadoEm);
    }

    /// <summary>
    /// Constroi as opcoes de tipo de relatorio exibidas na tela.
    /// </summary>
    private static IReadOnlyList<ReportOptionResponse> BuildReportTypeOptions()
    {
        return
        [
            new(ReportValues.ReportTypes.EstoqueAtual, "Estoque atual"),
            new(ReportValues.ReportTypes.PecasVendidas, "Pecas vendidas"),
            new(ReportValues.ReportTypes.Financeiro, "Financeiro"),
            new(ReportValues.ReportTypes.BaixasEstoque, "Baixas de estoque"),
        ];
    }

    /// <summary>
    /// Constroi as opcoes de status de peca usadas no relatorio de estoque.
    /// </summary>
    private static IReadOnlyList<ReportOptionResponse> BuildPieceStatusOptions()
    {
        return
        [
            new(PieceValues.PieceStatuses.Disponivel, "Disponivel"),
            new(PieceValues.PieceStatuses.Reservada, "Reservada"),
            new(PieceValues.PieceStatuses.Vendida, "Vendida"),
            new(PieceValues.PieceStatuses.Devolvida, "Devolvida"),
            new(PieceValues.PieceStatuses.Doada, "Doada"),
            new(PieceValues.PieceStatuses.Perdida, "Perdida"),
            new(PieceValues.PieceStatuses.Descartada, "Descartada"),
            new(PieceValues.PieceStatuses.Inativa, "Inativa"),
        ];
    }

    /// <summary>
    /// Constroi as opcoes de motivo usadas no relatorio de baixas.
    /// </summary>
    private static IReadOnlyList<ReportOptionResponse> BuildStockMovementReasonOptions()
    {
        return
        [
            new(PieceValues.StockMovementTypes.Devolucao, "Devolucao"),
            new(PieceValues.StockMovementTypes.Doacao, "Doacao"),
            new(PieceValues.StockMovementTypes.Perda, "Perda"),
            new(PieceValues.StockMovementTypes.Descarte, "Descarte"),
        ];
    }

    /// <summary>
    /// Normaliza o motivo de baixa aceito pelo relatorio.
    /// </summary>
    private static string NormalizeStockMovementReason(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized != PieceValues.StockMovementTypes.Devolucao &&
            normalized != PieceValues.StockMovementTypes.Doacao &&
            normalized != PieceValues.StockMovementTypes.Perda &&
            normalized != PieceValues.StockMovementTypes.Descarte)
        {
            throw new InvalidOperationException("Motivo de movimentacao invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Gera um nome de arquivo coerente com o relatorio retornado.
    /// </summary>
    private static string BuildFileName(ReportResultResponse result, string extension)
    {
        var slug = result.TipoRelatorio.Replace('_', '-');
        return $"{slug}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.{extension}";
    }

    /// <summary>
    /// Gera o CSV compativel com Excel para o relatorio selecionado.
    /// </summary>
    private static string BuildCsv(ReportResultResponse result)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(';', result.Colunas.Select(column => EscapeCsv(column.Titulo))));

        foreach (var row in result.Linhas)
        {
            var values = result.Colunas
                .Select(column => row.Celulas.FirstOrDefault(cell => cell.Chave == column.Chave)?.Valor ?? string.Empty)
                .Select(EscapeCsv);

            builder.AppendLine(string.Join(';', values));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gera o HTML imprimivel que pode ser salvo como PDF pelo navegador.
    /// </summary>
    private static string BuildPrintableHtml(ReportResultResponse result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\" />");
        builder.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;padding:24px;color:#1f2937}table{width:100%;border-collapse:collapse;margin-top:16px}th,td{border:1px solid #d1d5db;padding:8px;text-align:left;font-size:12px}th{background:#f3f4f6}h1{margin:0 0 6px;font-size:22px}p{margin:0 0 8px;color:#4b5563}.metrics{display:flex;gap:12px;flex-wrap:wrap;margin-top:16px}.metric{border:1px solid #d1d5db;border-radius:12px;padding:10px 12px;min-width:180px}</style>");
        builder.AppendLine("</head><body>");
        builder.AppendLine($"<h1>{WebUtility.HtmlEncode(result.Titulo)}</h1>");
        builder.AppendLine($"<p>{WebUtility.HtmlEncode(result.Subtitulo)}</p>");
        builder.AppendLine("<div class=\"metrics\">");

        foreach (var metric in result.Metricas)
        {
            builder.AppendLine("<div class=\"metric\">");
            builder.AppendLine($"<strong>{WebUtility.HtmlEncode(metric.Nome)}</strong><br />");
            builder.AppendLine($"<span>{WebUtility.HtmlEncode(metric.Valor)}</span>");
            builder.AppendLine("</div>");
        }

        builder.AppendLine("</div><table><thead><tr>");

        foreach (var column in result.Colunas)
        {
            builder.AppendLine($"<th>{WebUtility.HtmlEncode(column.Titulo)}</th>");
        }

        builder.AppendLine("</tr></thead><tbody>");

        foreach (var row in result.Linhas)
        {
            builder.AppendLine("<tr>");

            foreach (var column in result.Colunas)
            {
                var value = row.Celulas.FirstOrDefault(cell => cell.Chave == column.Chave)?.Valor ?? string.Empty;
                builder.AppendLine($"<td>{WebUtility.HtmlEncode(value)}</td>");
            }

            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody></table></body></html>");
        return builder.ToString();
    }

    private static string EscapeCsv(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return value.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    }

    private static DateTimeOffset ToUtcStart(DateOnly value)
    {
        return new DateTimeOffset(value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    private static DateTimeOffset ToUtcEnd(DateOnly value)
    {
        return new DateTimeOffset(value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
    }
}
