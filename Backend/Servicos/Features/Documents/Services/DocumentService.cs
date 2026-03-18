using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Documents.Abstractions;
using Renova.Services.Features.Documents.Contracts;
using Renova.Services.Features.Pieces;

namespace Renova.Services.Features.Documents.Services;

// Implementa o modulo 16 com busca e impressao unificada de documentos.
public sealed class DocumentService : IDocumentService
{
    private readonly RenovaDbContext _dbContext;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia e contexto autenticado.
    /// </summary>
    public DocumentService(
        RenovaDbContext dbContext,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega a loja ativa e os tipos de documento disponiveis.
    /// </summary>
    public async Task<DocumentWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureAnyDocumentContextAsync(cancellationToken);

        return new DocumentWorkspaceResponse(
            context.LojaId,
            context.StoreName,
            DocumentValues.BuildDocumentTypes()
                .Select(x => new DocumentTypeOptionResponse(x.Codigo, x.Nome, x.Descricao))
                .ToArray());
    }

    /// <summary>
    /// Busca pecas elegiveis para impressao de etiqueta.
    /// </summary>
    public async Task<IReadOnlyList<DocumentSearchItemResponse>> BuscarEtiquetasAsync(
        DocumentSearchQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsurePieceDocumentContextAsync(cancellationToken);
        var term = NormalizeSearch(query.Search);

        return await (
                from piece in _dbContext.Pecas.AsNoTracking()
                join product in _dbContext.ProdutoNomes.AsNoTracking() on piece.ProdutoNomeId equals product.Id
                join brand in _dbContext.Marcas.AsNoTracking() on piece.MarcaId equals brand.Id
                where piece.LojaId == context.LojaId
                where term == null ||
                      piece.CodigoInterno.ToLower().Contains(term) ||
                      piece.CodigoBarras.ToLower().Contains(term) ||
                      product.Nome.ToLower().Contains(term) ||
                      brand.Nome.ToLower().Contains(term)
                orderby piece.CodigoInterno
                select new DocumentSearchItemResponse(
                    piece.Id,
                    piece.CodigoInterno,
                    $"{product.Nome} • {brand.Nome}",
                    piece.CodigoBarras))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca vendas para impressao de recibo.
    /// </summary>
    public async Task<IReadOnlyList<DocumentSearchItemResponse>> BuscarRecibosVendaAsync(
        DocumentSearchQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureSaleDocumentContextAsync(cancellationToken);
        var term = NormalizeSearch(query.Search);

        return await (
                from sale in _dbContext.Vendas.AsNoTracking()
                join buyer in _dbContext.Pessoas.AsNoTracking() on sale.CompradorPessoaId equals buyer.Id into buyerGroup
                from buyer in buyerGroup.DefaultIfEmpty()
                join seller in _dbContext.Usuarios.AsNoTracking() on sale.VendedorUsuarioId equals seller.Id
                where sale.LojaId == context.LojaId
                where term == null ||
                      sale.NumeroVenda.ToLower().Contains(term) ||
                      (buyer != null && buyer.Nome.ToLower().Contains(term)) ||
                      seller.Nome.ToLower().Contains(term)
                orderby sale.DataHoraVenda descending
                select new DocumentSearchItemResponse(
                    sale.Id,
                    sale.NumeroVenda,
                    $"{(buyer != null ? buyer.Nome : "Sem comprador")} • {FormatDate(sale.DataHoraVenda)}",
                    $"{sale.StatusVenda} • {FormatCurrency(sale.TotalLiquido)}"))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca obrigacoes com liquidacao para comprovante ao fornecedor.
    /// </summary>
    public async Task<IReadOnlyList<DocumentSearchItemResponse>> BuscarComprovantesFornecedorAsync(
        DocumentSearchQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureSupplierPaymentDocumentContextAsync(cancellationToken);
        var term = NormalizeSearch(query.Search);

        return await (
                from obligation in _dbContext.ObrigacoesFornecedor.AsNoTracking()
                join supplier in _dbContext.Pessoas.AsNoTracking() on obligation.PessoaId equals supplier.Id
                where obligation.LojaId == context.LojaId
                where _dbContext.LiquidacoesObrigacaoFornecedor.Any(x => x.ObrigacaoFornecedorId == obligation.Id)
                where term == null ||
                      supplier.Nome.ToLower().Contains(term) ||
                      supplier.Documento.ToLower().Contains(term) ||
                      obligation.Observacoes.ToLower().Contains(term)
                orderby obligation.DataGeracao descending
                select new DocumentSearchItemResponse(
                    obligation.Id,
                    supplier.Nome,
                    $"{obligation.TipoObrigacao} • {obligation.StatusObrigacao}",
                    FormatCurrency(obligation.ValorOriginal)))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca devolucoes e doacoes para impressao do comprovante.
    /// </summary>
    public async Task<IReadOnlyList<DocumentSearchItemResponse>> BuscarComprovantesConsignacaoAsync(
        DocumentSearchQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsurePieceDocumentContextAsync(cancellationToken);
        var term = NormalizeSearch(query.Search);

        return await (
                from movement in _dbContext.MovimentacoesEstoque.AsNoTracking()
                join piece in _dbContext.Pecas.AsNoTracking() on movement.PecaId equals piece.Id
                join product in _dbContext.ProdutoNomes.AsNoTracking() on piece.ProdutoNomeId equals product.Id
                where movement.LojaId == context.LojaId
                where movement.TipoMovimentacao == PieceValues.StockMovementTypes.Devolucao ||
                      movement.TipoMovimentacao == PieceValues.StockMovementTypes.Doacao
                where term == null ||
                      piece.CodigoInterno.ToLower().Contains(term) ||
                      product.Nome.ToLower().Contains(term) ||
                      movement.Motivo.ToLower().Contains(term)
                orderby movement.MovimentadoEm descending
                select new DocumentSearchItemResponse(
                    piece.Id,
                    piece.CodigoInterno,
                    $"{product.Nome} • {movement.TipoMovimentacao}",
                    FormatDate(movement.MovimentadoEm)))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gera a etiqueta unificada da peca com codigo e codigo de barras.
    /// </summary>
    public async Task<PrintableDocumentFileResponse> ImprimirEtiquetaAsync(
        Guid pecaId,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsurePieceDocumentContextAsync(cancellationToken);

        var piece = await (
                from item in _dbContext.Pecas.AsNoTracking()
                join product in _dbContext.ProdutoNomes.AsNoTracking() on item.ProdutoNomeId equals product.Id
                join brand in _dbContext.Marcas.AsNoTracking() on item.MarcaId equals brand.Id
                join size in _dbContext.Tamanhos.AsNoTracking() on item.TamanhoId equals size.Id
                join color in _dbContext.Cores.AsNoTracking() on item.CorId equals color.Id
                join supplier in _dbContext.Pessoas.AsNoTracking() on item.FornecedorPessoaId equals supplier.Id into supplierGroup
                from supplier in supplierGroup.DefaultIfEmpty()
                where item.Id == pecaId && item.LojaId == context.LojaId
                select new
                {
                    item.CodigoInterno,
                    item.CodigoBarras,
                    item.PrecoVendaAtual,
                    item.TipoPeca,
                    item.StatusPeca,
                    item.LocalizacaoFisica,
                    ProdutoNome = product.Nome,
                    MarcaNome = brand.Nome,
                    TamanhoNome = size.Nome,
                    CorNome = color.Nome,
                    FornecedorNome = supplier != null ? supplier.Nome : "Sem fornecedor",
                })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Peca nao encontrada para impressao.");

        var barcodeValue = string.IsNullOrWhiteSpace(piece.CodigoBarras)
            ? piece.CodigoInterno
            : piece.CodigoBarras;

        var html = BuildLabelHtml(
            context.StoreName,
            piece.CodigoInterno,
            barcodeValue,
            piece.ProdutoNome,
            piece.MarcaNome,
            piece.TamanhoNome,
            piece.CorNome,
            piece.FornecedorNome,
            piece.TipoPeca,
            piece.StatusPeca,
            piece.LocalizacaoFisica,
            piece.PrecoVendaAtual);

        return BuildHtmlFile($"etiqueta-{piece.CodigoInterno}.html", html);
    }

    /// <summary>
    /// Gera o recibo unificado de uma venda concluida ou cancelada.
    /// </summary>
    public async Task<PrintableDocumentFileResponse> ImprimirReciboVendaAsync(
        Guid vendaId,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureSaleDocumentContextAsync(cancellationToken);

        var sale = await (
                from item in _dbContext.Vendas.AsNoTracking()
                join seller in _dbContext.Usuarios.AsNoTracking() on item.VendedorUsuarioId equals seller.Id
                join buyer in _dbContext.Pessoas.AsNoTracking() on item.CompradorPessoaId equals buyer.Id into buyerGroup
                from buyer in buyerGroup.DefaultIfEmpty()
                where item.Id == vendaId && item.LojaId == context.LojaId
                select new
                {
                    item.Id,
                    item.NumeroVenda,
                    item.StatusVenda,
                    item.DataHoraVenda,
                    item.Subtotal,
                    item.DescontoTotal,
                    item.TaxaTotal,
                    item.TotalLiquido,
                    item.Observacoes,
                    item.MotivoCancelamento,
                    VendedorNome = seller.Nome,
                    CompradorNome = buyer != null ? buyer.Nome : "Sem comprador",
                    CompradorDocumento = buyer != null ? buyer.Documento : string.Empty,
                })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Venda nao encontrada para impressao.");

        var items = await (
                from saleItem in _dbContext.VendaItens.AsNoTracking()
                join piece in _dbContext.Pecas.AsNoTracking() on saleItem.PecaId equals piece.Id
                join product in _dbContext.ProdutoNomes.AsNoTracking() on piece.ProdutoNomeId equals product.Id
                join brand in _dbContext.Marcas.AsNoTracking() on piece.MarcaId equals brand.Id
                where saleItem.VendaId == sale.Id
                orderby piece.CodigoInterno
                select new ReceiptLine(
                    piece.CodigoInterno,
                    $"{product.Nome} / {brand.Nome}",
                    saleItem.Quantidade.ToString(CultureInfo.InvariantCulture),
                    FormatCurrency(saleItem.PrecoFinalUnitario),
                    FormatCurrency(saleItem.PrecoFinalUnitario * saleItem.Quantidade)))
            .ToListAsync(cancellationToken);

        var payments = await (
                from payment in _dbContext.VendaPagamentos.AsNoTracking()
                join method in _dbContext.MeiosPagamento.AsNoTracking() on payment.MeioPagamentoId equals method.Id into methodGroup
                from method in methodGroup.DefaultIfEmpty()
                where payment.VendaId == sale.Id
                orderby payment.Sequencia
                select new ReceiptLine(
                    payment.Sequencia.ToString(CultureInfo.InvariantCulture),
                    method != null ? method.Nome : payment.TipoPagamento,
                    payment.TipoPagamento,
                    FormatCurrency(payment.Valor),
                    FormatCurrency(payment.ValorLiquido)))
            .ToListAsync(cancellationToken);

        var html = CreateReceiptDocument(
            "Recibo de venda",
            context.StoreName,
            sale.NumeroVenda,
            sale.DataHoraVenda,
            new[]
            {
                ("Status", sale.StatusVenda),
                ("Comprador", sale.CompradorNome),
                ("Documento", string.IsNullOrWhiteSpace(sale.CompradorDocumento) ? "-" : sale.CompradorDocumento),
                ("Vendedor", sale.VendedorNome),
            },
            [
                BuildReceiptTable(
                    "Itens da venda",
                    ["Codigo", "Produto", "Qtd.", "Unitario", "Total"],
                    items),
                BuildReceiptTable(
                    "Pagamentos",
                    ["Seq.", "Meio", "Tipo", "Valor", "Liquido"],
                    payments),
            ],
            BuildTotalsHtml(
                [
                    ("Subtotal", sale.Subtotal),
                    ("Desconto", sale.DescontoTotal),
                    ("Taxa", sale.TaxaTotal),
                    ("Total liquido", sale.TotalLiquido),
                ]),
            sale.Observacoes,
            sale.MotivoCancelamento);

        return BuildHtmlFile($"recibo-venda-{sale.NumeroVenda}.html", html);
    }

    /// <summary>
    /// Gera o comprovante unificado de liquidacao ao fornecedor.
    /// </summary>
    public async Task<PrintableDocumentFileResponse> ImprimirComprovanteFornecedorAsync(
        Guid obrigacaoId,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureSupplierPaymentDocumentContextAsync(cancellationToken);

        var obligation = await (
                from item in _dbContext.ObrigacoesFornecedor.AsNoTracking()
                join supplier in _dbContext.Pessoas.AsNoTracking() on item.PessoaId equals supplier.Id
                join piece in _dbContext.Pecas.AsNoTracking() on item.PecaId equals piece.Id into pieceGroup
                from piece in pieceGroup.DefaultIfEmpty()
                join saleItem in _dbContext.VendaItens.AsNoTracking() on item.VendaItemId equals saleItem.Id into saleItemGroup
                from saleItem in saleItemGroup.DefaultIfEmpty()
                join sale in _dbContext.Vendas.AsNoTracking() on saleItem.VendaId equals sale.Id into saleGroup
                from sale in saleGroup.DefaultIfEmpty()
                where item.Id == obrigacaoId && item.LojaId == context.LojaId
                select new
                {
                    item.Id,
                    item.TipoObrigacao,
                    item.StatusObrigacao,
                    item.DataGeracao,
                    item.DataVencimento,
                    item.ValorOriginal,
                    item.ValorEmAberto,
                    item.Observacoes,
                    FornecedorNome = supplier.Nome,
                    FornecedorDocumento = supplier.Documento,
                    CodigoInterno = piece != null ? piece.CodigoInterno : null,
                    NumeroVenda = sale != null ? sale.NumeroVenda : null,
                })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Obrigacao do fornecedor nao encontrada para impressao.");

        var liquidations = await (
                from settlement in _dbContext.LiquidacoesObrigacaoFornecedor.AsNoTracking()
                join user in _dbContext.Usuarios.AsNoTracking() on settlement.LiquidadoPorUsuarioId equals user.Id
                join method in _dbContext.MeiosPagamento.AsNoTracking() on settlement.MeioPagamentoId equals method.Id into methodGroup
                from method in methodGroup.DefaultIfEmpty()
                where settlement.ObrigacaoFornecedorId == obligation.Id
                orderby settlement.LiquidadoEm
                select new
                {
                    settlement.TipoLiquidacao,
                    MeioPagamentoNome = method != null ? method.Nome : "Credito da loja",
                    settlement.Valor,
                    settlement.LiquidadoEm,
                    ResponsavelNome = user.Nome,
                })
            .ToListAsync(cancellationToken);

        if (liquidations.Count == 0)
        {
            throw new InvalidOperationException("A obrigacao ainda nao possui liquidacao para impressao.");
        }

        var html = CreateReceiptDocument(
            "Comprovante de pagamento ao fornecedor",
            context.StoreName,
            obligation.Id.ToString(),
            liquidations.Max(x => x.LiquidadoEm),
            new[]
            {
                ("Fornecedor", obligation.FornecedorNome),
                ("Documento", obligation.FornecedorDocumento),
                ("Tipo", obligation.TipoObrigacao),
                ("Status", obligation.StatusObrigacao),
                ("Peca", string.IsNullOrWhiteSpace(obligation.CodigoInterno) ? "-" : obligation.CodigoInterno!),
                ("Venda", string.IsNullOrWhiteSpace(obligation.NumeroVenda) ? "-" : obligation.NumeroVenda!),
            },
            [
                BuildReceiptTable(
                    "Liquidacoes",
                    ["Data", "Tipo", "Meio", "Valor", "Responsavel"],
                    liquidations.Select(x => new ReceiptLine(
                        FormatDate(x.LiquidadoEm),
                        x.TipoLiquidacao,
                        x.MeioPagamentoNome,
                        FormatCurrency(x.Valor),
                        x.ResponsavelNome)).ToArray())
            ],
            BuildTotalsHtml(
                [
                    ("Valor original", obligation.ValorOriginal),
                    ("Valor liquidado", liquidations.Sum(x => x.Valor)),
                    ("Em aberto", obligation.ValorEmAberto),
                ]),
            obligation.Observacoes,
            null);

        return BuildHtmlFile($"comprovante-fornecedor-{obligation.Id:N}.html", html);
    }

    /// <summary>
    /// Gera o comprovante unificado de devolucao ou doacao de consignacao.
    /// </summary>
    public async Task<PrintableDocumentFileResponse> ImprimirComprovanteConsignacaoAsync(
        Guid pecaId,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsurePieceDocumentContextAsync(cancellationToken);

        var piece = await (
                from item in _dbContext.Pecas.AsNoTracking()
                join product in _dbContext.ProdutoNomes.AsNoTracking() on item.ProdutoNomeId equals product.Id
                join brand in _dbContext.Marcas.AsNoTracking() on item.MarcaId equals brand.Id
                join supplier in _dbContext.Pessoas.AsNoTracking() on item.FornecedorPessoaId equals supplier.Id into supplierGroup
                from supplier in supplierGroup.DefaultIfEmpty()
                where item.Id == pecaId && item.LojaId == context.LojaId
                select new
                {
                    item.Id,
                    item.CodigoInterno,
                    ProdutoNome = product.Nome,
                    MarcaNome = brand.Nome,
                    FornecedorNome = supplier != null ? supplier.Nome : "Sem fornecedor",
                })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Peca nao encontrada para impressao.");

        var movement = await (
                from item in _dbContext.MovimentacoesEstoque.AsNoTracking()
                join user in _dbContext.Usuarios.AsNoTracking() on item.MovimentadoPorUsuarioId equals user.Id
                where item.PecaId == piece.Id
                where item.LojaId == context.LojaId
                where item.TipoMovimentacao == PieceValues.StockMovementTypes.Devolucao ||
                      item.TipoMovimentacao == PieceValues.StockMovementTypes.Doacao
                orderby item.MovimentadoEm descending
                select new
                {
                    item.TipoMovimentacao,
                    item.Quantidade,
                    item.Motivo,
                    item.MovimentadoEm,
                    ResponsavelNome = user.Nome,
                })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Nao existe devolucao ou doacao registrada para esta peca.");

        var html = CreateReceiptDocument(
            "Comprovante de devolucao ou doacao",
            context.StoreName,
            piece.CodigoInterno,
            movement.MovimentadoEm,
            new[]
            {
                ("Tipo", movement.TipoMovimentacao),
                ("Peca", piece.CodigoInterno),
                ("Produto", $"{piece.ProdutoNome} / {piece.MarcaNome}"),
                ("Fornecedor", piece.FornecedorNome),
                ("Responsavel", movement.ResponsavelNome),
            },
            [
                BuildReceiptTable(
                    "Movimentacao",
                    ["Data", "Tipo", "Quantidade", "Motivo", "Responsavel"],
                    [
                        new ReceiptLine(
                            FormatDate(movement.MovimentadoEm),
                            movement.TipoMovimentacao,
                            Math.Abs(movement.Quantidade).ToString(CultureInfo.InvariantCulture),
                            movement.Motivo,
                            movement.ResponsavelNome),
                    ])
            ],
            null,
            movement.Motivo,
            null);

        return BuildHtmlFile($"comprovante-consignacao-{piece.CodigoInterno}.html", html);
    }

    /// <summary>
    /// Exige autenticacao, loja ativa valida e ao menos uma permissao do modulo.
    /// </summary>
    private async Task<DocumentContext> EnsureAnyDocumentContextAsync(CancellationToken cancellationToken)
    {
        return await EnsureStoreContextAsync(
            [
                AccessPermissionCodes.PecasVisualizar,
                AccessPermissionCodes.PecasCadastrar,
                AccessPermissionCodes.PecasAjustar,
                AccessPermissionCodes.VendasRegistrar,
                AccessPermissionCodes.VendasCancelar,
                AccessPermissionCodes.FinanceiroVisualizar,
                AccessPermissionCodes.FinanceiroConciliar,
            ],
            "Voce nao possui acesso ao modulo de impressoes e documentos.",
            cancellationToken);
    }

    /// <summary>
    /// Exige contexto valido para etiquetas e comprovantes de consignacao.
    /// </summary>
    private async Task<DocumentContext> EnsurePieceDocumentContextAsync(CancellationToken cancellationToken)
    {
        return await EnsureStoreContextAsync(
            [
                AccessPermissionCodes.PecasVisualizar,
                AccessPermissionCodes.PecasCadastrar,
                AccessPermissionCodes.PecasAjustar,
            ],
            "Voce nao possui acesso aos documentos de pecas.",
            cancellationToken);
    }

    /// <summary>
    /// Exige contexto valido para recibos de venda.
    /// </summary>
    private async Task<DocumentContext> EnsureSaleDocumentContextAsync(CancellationToken cancellationToken)
    {
        return await EnsureStoreContextAsync(
            [
                AccessPermissionCodes.VendasRegistrar,
                AccessPermissionCodes.VendasCancelar,
            ],
            "Voce nao possui acesso aos recibos de venda.",
            cancellationToken);
    }

    /// <summary>
    /// Exige contexto valido para comprovantes financeiros do fornecedor.
    /// </summary>
    private async Task<DocumentContext> EnsureSupplierPaymentDocumentContextAsync(CancellationToken cancellationToken)
    {
        return await EnsureStoreContextAsync(
            [
                AccessPermissionCodes.FinanceiroVisualizar,
                AccessPermissionCodes.FinanceiroConciliar,
            ],
            "Voce nao possui acesso aos comprovantes financeiros.",
            cancellationToken);
    }

    /// <summary>
    /// Valida autenticacao, loja ativa e vinculacao do usuario antes de consultar documentos.
    /// </summary>
    private async Task<DocumentContext> EnsureStoreContextAsync(
        IReadOnlyCollection<string> permissionCodes,
        string deniedMessage,
        CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja para continuar.");

        var store = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == lojaId && x.Ativo, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var hasMembership = await _dbContext.UsuarioLojas.AnyAsync(
            x => x.UsuarioId == usuarioId &&
                 x.LojaId == lojaId &&
                 x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                 (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
            cancellationToken);

        if (!hasMembership)
        {
            throw new InvalidOperationException("Voce nao possui acesso a loja ativa.");
        }

        var hasPermission = await HasAnyPermissionAsync(usuarioId, lojaId, permissionCodes, cancellationToken);
        if (!hasPermission)
        {
            throw new InvalidOperationException(deniedMessage);
        }

        return new DocumentContext(usuarioId, lojaId, store.NomeFantasia);
    }

    /// <summary>
    /// Consulta se o usuario possui ao menos uma permissao efetiva na loja.
    /// </summary>
    private async Task<bool> HasAnyPermissionAsync(
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
    /// Monta o HTML da etiqueta padrao do sistema.
    /// </summary>
    private static string BuildLabelHtml(
        string storeName,
        string internalCode,
        string barcodeValue,
        string productName,
        string brandName,
        string sizeName,
        string colorName,
        string supplierName,
        string pieceType,
        string pieceStatus,
        string location,
        decimal salePrice)
    {
        var svg = BuildCode39Svg(barcodeValue);

        return $$"""
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8" />
  <title>Etiqueta {{Html(internalCode)}}</title>
  <style>
    body{font-family:'Segoe UI',Arial,sans-serif;background:#f5f1e8;margin:0;padding:24px;color:#261b16}
    .label{width:420px;background:#fff;border:1px solid #d8cbb9;border-radius:18px;padding:22px;box-shadow:0 14px 36px rgba(73,46,28,.12)}
    .eyebrow{font-size:11px;letter-spacing:.16em;text-transform:uppercase;color:#8d6d58}
    .code{font-size:24px;font-weight:700;margin:6px 0 12px}
    .title{font-size:20px;font-weight:700;margin:0 0 6px}
    .copy{font-size:13px;color:#6b5749;margin:0 0 14px}
    .price{display:inline-block;background:#261b16;color:#fff;padding:10px 14px;border-radius:999px;font-size:20px;font-weight:700;margin:10px 0 14px}
    .meta{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:10px;margin-top:14px}
    .meta-item{border:1px solid #eadfce;border-radius:12px;padding:10px 12px;background:#fcfaf6}
    .meta-label{font-size:10px;letter-spacing:.08em;text-transform:uppercase;color:#9a806c;margin-bottom:4px}
    .meta-value{font-size:13px;font-weight:600}
    .barcode{margin-top:18px;padding-top:16px;border-top:1px dashed #d9c7b3}
    .barcode-code{font-size:12px;letter-spacing:.22em;text-align:center;color:#5d4738;margin-top:8px}
  </style>
</head>
<body>
  <main class="label">
    <div class="eyebrow">{{Html(storeName)}}</div>
    <div class="code">{{Html(internalCode)}}</div>
    <h1 class="title">{{Html(productName)}}</h1>
    <p class="copy">{{Html(brandName)}} • {{Html(sizeName)}} • {{Html(colorName)}}</p>
    <div class="price">{{Html(FormatCurrency(salePrice))}}</div>
    <div class="meta">
      <div class="meta-item"><div class="meta-label">Fornecedor</div><div class="meta-value">{{Html(supplierName)}}</div></div>
      <div class="meta-item"><div class="meta-label">Tipo</div><div class="meta-value">{{Html(pieceType)}}</div></div>
      <div class="meta-item"><div class="meta-label">Status</div><div class="meta-value">{{Html(pieceStatus)}}</div></div>
      <div class="meta-item"><div class="meta-label">Localizacao</div><div class="meta-value">{{Html(string.IsNullOrWhiteSpace(location) ? "-" : location)}}</div></div>
    </div>
    <section class="barcode">
      {{svg}}
      <div class="barcode-code">{{Html(barcodeValue)}}</div>
    </section>
  </main>
</body>
</html>
""";
    }

    /// <summary>
    /// Monta o HTML unificado usado por recibos e comprovantes.
    /// </summary>
    private static string CreateReceiptDocument(
        string documentTitle,
        string storeName,
        string documentCode,
        DateTimeOffset issuedAt,
        IReadOnlyCollection<(string Label, string Value)> highlights,
        IReadOnlyCollection<string> sections,
        string? totalsHtml,
        string? observations,
        string? secondaryNote)
    {
        var highlightHtml = string.Join(
            string.Empty,
            highlights.Select(item =>
                $"<div class=\"highlight\"><div class=\"highlight-label\">{Html(item.Label)}</div><div class=\"highlight-value\">{Html(item.Value)}</div></div>"));
        var sectionsHtml = string.Join(string.Empty, sections);
        var notes = new List<string>();

        if (!string.IsNullOrWhiteSpace(observations))
        {
            notes.Add($"<div class=\"note\"><strong>Observacoes:</strong><br />{Html(observations)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(secondaryNote))
        {
            notes.Add($"<div class=\"note\"><strong>Detalhe:</strong><br />{Html(secondaryNote)}</div>");
        }

        return $$"""
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8" />
  <title>{{Html(documentTitle)}}</title>
  <style>
    body{font-family:'Segoe UI',Arial,sans-serif;background:#f7f3eb;margin:0;padding:28px;color:#2f251f}
    .sheet{max-width:960px;margin:0 auto;background:#fff;border:1px solid #ddcfbf;border-radius:24px;overflow:hidden;box-shadow:0 16px 48px rgba(61,42,25,.12)}
    .hero{padding:28px 30px 18px;background:linear-gradient(135deg,#f3e4d2,#fbf7f1)}
    .eyebrow{font-size:11px;letter-spacing:.18em;text-transform:uppercase;color:#9c7860}
    .title{font-size:28px;font-weight:700;margin:8px 0 6px}
    .copy{font-size:14px;color:#6a5647;margin:0}
    .meta-bar{display:flex;gap:12px;flex-wrap:wrap;padding:18px 30px;border-bottom:1px solid #efe3d5;background:#fffdfb}
    .meta-pill{border:1px solid #e8d8c6;border-radius:999px;padding:10px 14px;background:#fff}
    .meta-label{font-size:10px;letter-spacing:.08em;text-transform:uppercase;color:#8d7361}
    .meta-value{font-size:14px;font-weight:700;margin-top:2px}
    .content{padding:24px 30px 30px}
    .highlight-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:12px;margin-bottom:18px}
    .highlight{border:1px solid #eadfce;border-radius:14px;padding:12px 14px;background:#fcfaf6}
    .highlight-label{font-size:11px;letter-spacing:.08em;text-transform:uppercase;color:#90735d;margin-bottom:6px}
    .highlight-value{font-size:15px;font-weight:600}
    .section{margin-top:18px}
    .section h2{font-size:15px;margin:0 0 10px}
    table{width:100%;border-collapse:collapse}
    th,td{padding:10px 12px;border:1px solid #eadfce;font-size:12px;text-align:left;vertical-align:top}
    th{background:#f8efe4;font-size:11px;letter-spacing:.06em;text-transform:uppercase;color:#725a48}
    .totals{margin-top:18px;margin-left:auto;max-width:320px;border:1px solid #eadfce;border-radius:16px;padding:14px;background:#fcfaf6}
    .total-row{display:flex;justify-content:space-between;gap:14px;padding:4px 0;font-size:14px}
    .total-row strong{font-size:16px}
    .notes{margin-top:18px;display:grid;gap:12px}
    .note{border:1px dashed #d7c6b2;border-radius:14px;padding:12px;background:#fffdf9;font-size:13px;line-height:1.5}
  </style>
</head>
<body>
  <main class="sheet">
    <header class="hero">
      <div class="eyebrow">{{Html(storeName)}}</div>
      <h1 class="title">{{Html(documentTitle)}}</h1>
      <p class="copy">Modelo unico de comprovante do Renova para impressao rapida da operacao.</p>
    </header>
    <section class="meta-bar">
      <div class="meta-pill"><div class="meta-label">Documento</div><div class="meta-value">{{Html(documentCode)}}</div></div>
      <div class="meta-pill"><div class="meta-label">Emitido em</div><div class="meta-value">{{Html(FormatDate(issuedAt))}}</div></div>
    </section>
    <section class="content">
      <div class="highlight-grid">{{highlightHtml}}</div>
      {{sectionsHtml}}
      {{totalsHtml ?? string.Empty}}
      <div class="notes">{{string.Join(string.Empty, notes)}}</div>
    </section>
  </main>
</body>
</html>
""";
    }

    /// <summary>
    /// Monta uma tabela padrao para as secoes do comprovante.
    /// </summary>
    private static string BuildReceiptTable(
        string title,
        IReadOnlyList<string> headers,
        IReadOnlyCollection<ReceiptLine> lines)
    {
        var headerHtml = string.Join(string.Empty, headers.Select(item => $"<th>{Html(item)}</th>"));
        var rowHtml = string.Join(
            string.Empty,
            lines.Select(line =>
                $"<tr><td>{Html(line.Coluna1)}</td><td>{Html(line.Coluna2)}</td><td>{Html(line.Coluna3)}</td><td>{Html(line.Coluna4)}</td><td>{Html(line.Coluna5)}</td></tr>"));

        return $"<section class=\"section\"><h2>{Html(title)}</h2><table><thead><tr>{headerHtml}</tr></thead><tbody>{rowHtml}</tbody></table></section>";
    }

    /// <summary>
    /// Monta o quadro de totais padrao usado pelos comprovantes financeiros.
    /// </summary>
    private static string BuildTotalsHtml(IReadOnlyCollection<(string Label, decimal Value)> totals)
    {
        var totalItems = totals.ToList();
        if (totalItems.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.Append("<section class=\"totals\">");

        for (var index = 0; index < totalItems.Count; index++)
        {
            var total = totalItems[index];
            var valueTag = index == totalItems.Count - 1 ? "strong" : "span";
            builder.Append("<div class=\"total-row\">");
            builder.Append($"<span>{Html(total.Label)}</span>");
            builder.Append($"<{valueTag}>{Html(FormatCurrency(total.Value))}</{valueTag}>");
            builder.Append("</div>");
        }

        builder.Append("</section>");
        return builder.ToString();
    }

    /// <summary>
    /// Converte o HTML montado no payload de arquivo da API.
    /// </summary>
    private static PrintableDocumentFileResponse BuildHtmlFile(string fileName, string html)
    {
        return new PrintableDocumentFileResponse(
            fileName,
            "text/html; charset=utf-8",
            Encoding.UTF8.GetBytes(html));
    }

    /// <summary>
    /// Gera um SVG simples no padrao visual de codigo de barras para a etiqueta.
    /// </summary>
    private static string BuildCode39Svg(string value)
    {
        var normalized = NormalizeBarcodeValue(value);
        var patterns = new Dictionary<char, string>
        {
            ['0'] = "nnnwwnwnn",
            ['1'] = "wnnwnnnnw",
            ['2'] = "nnwwnnnnw",
            ['3'] = "wnwwnnnnn",
            ['4'] = "nnnwwnnnw",
            ['5'] = "wnnwwnnnn",
            ['6'] = "nnwwwnnnn",
            ['7'] = "nnnwnnwnw",
            ['8'] = "wnnwnnwnn",
            ['9'] = "nnwwnnwnn",
            ['A'] = "wnnnnwnnw",
            ['B'] = "nnwnnwnnw",
            ['C'] = "wnwnnwnnn",
            ['D'] = "nnnnwwnnw",
            ['E'] = "wnnnwwnnn",
            ['F'] = "nnwnwwnnn",
            ['G'] = "nnnnnwwnw",
            ['H'] = "wnnnnwwnn",
            ['I'] = "nnwnnwwnn",
            ['J'] = "nnnnwwwnn",
            ['K'] = "wnnnnnnww",
            ['L'] = "nnwnnnnww",
            ['M'] = "wnwnnnnwn",
            ['N'] = "nnnnwnnww",
            ['O'] = "wnnnwnnwn",
            ['P'] = "nnwnwnnwn",
            ['Q'] = "nnnnnnwww",
            ['R'] = "wnnnnnwwn",
            ['S'] = "nnwnnnwwn",
            ['T'] = "nnnnwnwwn",
            ['U'] = "wwnnnnnnw",
            ['V'] = "nwwnnnnnw",
            ['W'] = "wwwnnnnnn",
            ['X'] = "nwnnwnnnw",
            ['Y'] = "wwnnwnnnn",
            ['Z'] = "nwwnwnnnn",
            ['-'] = "nwnnnnwnw",
            ['.'] = "wwnnnnwnn",
            [' '] = "nwwnnnwnn",
            ['*'] = "nwnnwnwnn",
        };

        var fullValue = $"*{normalized}*";
        var x = 0;
        var builder = new StringBuilder();
        builder.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"320\" height=\"88\" viewBox=\"0 0 320 88\" role=\"img\" aria-label=\"Codigo de barras\">");
        builder.Append("<rect width=\"320\" height=\"88\" fill=\"#ffffff\"/>");

        foreach (var character in fullValue)
        {
            if (!patterns.TryGetValue(character, out var pattern))
            {
                continue;
            }

            for (var index = 0; index < pattern.Length; index++)
            {
                var unitWidth = pattern[index] == 'w' ? 6 : 2;
                var isBar = index % 2 == 0;
                if (isBar)
                {
                    builder.Append($"<rect x=\"{x}\" y=\"4\" width=\"{unitWidth}\" height=\"64\" fill=\"#261b16\" rx=\"1\" />");
                }

                x += unitWidth;
            }

            x += 2;
        }

        builder.Append("</svg>");
        return builder.ToString();
    }

    /// <summary>
    /// Sanitiza o conteudo impresso no codigo de barras.
    /// </summary>
    private static string NormalizeBarcodeValue(string value)
    {
        var normalized = new string(
            value
                .Trim()
                .ToUpperInvariant()
                .Where(character => char.IsLetterOrDigit(character) || character is '-' or '.' or ' ')
                .ToArray());

        return string.IsNullOrWhiteSpace(normalized) ? "SEM-CODIGO" : normalized;
    }

    /// <summary>
    /// Normaliza o termo de busca livre das listagens do modulo.
    /// </summary>
    private static string? NormalizeSearch(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Escapa texto dinamico antes de inserir no HTML imprimivel.
    /// </summary>
    private static string Html(string value)
    {
        return WebUtility.HtmlEncode(value);
    }

    /// <summary>
    /// Formata valores monetarios no padrao pt-BR.
    /// </summary>
    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C2", new CultureInfo("pt-BR"));
    }

    /// <summary>
    /// Formata datas locais para exibicao nos documentos.
    /// </summary>
    private static string FormatDate(DateTimeOffset value)
    {
        return value.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    }

    private sealed record DocumentContext(Guid UsuarioId, Guid LojaId, string StoreName);

    private sealed record ReceiptLine(
        string Coluna1,
        string Coluna2,
        string Coluna3,
        string Coluna4,
        string Coluna5);
}
