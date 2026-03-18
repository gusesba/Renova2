using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.CommercialRules;
using Renova.Services.Features.CommercialRules.Abstractions;
using Renova.Services.Features.CommercialRules.Contracts;
using Renova.Services.Features.Pieces.Abstractions;
using Renova.Services.Features.Pieces.Contracts;
using Renova.Services.Features.SupplierPayments;

namespace Renova.Services.Features.Pieces.Services;

// Implementa o modulo 06 com cadastro de pecas, snapshot comercial e entrada inicial de estoque.
public sealed class PieceService : IPieceService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;
    private readonly ICommercialRuleResolverService _commercialRuleResolverService;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria, contexto e resolvedor comercial.
    /// </summary>
    public PieceService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext,
        ICommercialRuleResolverService commercialRuleResolverService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
        _commercialRuleResolverService = commercialRuleResolverService;
    }

    /// <summary>
    /// Carrega os cadastros auxiliares e opcoes usadas no cadastro de pecas.
    /// </summary>
    public async Task<PieceWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanViewPiecesAsync(context.UsuarioId, context.LojaId, cancellationToken);

        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var produtoNomes = await _dbContext.ProdutoNomes
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderBy(x => x.Nome)
            .Select(x => new PieceCatalogOptionResponse(x.Id, x.Nome))
            .ToListAsync(cancellationToken);

        var marcas = await _dbContext.Marcas
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderBy(x => x.Nome)
            .Select(x => new PieceCatalogOptionResponse(x.Id, x.Nome))
            .ToListAsync(cancellationToken);

        var tamanhos = await _dbContext.Tamanhos
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderBy(x => x.Nome)
            .Select(x => new PieceCatalogOptionResponse(x.Id, x.Nome))
            .ToListAsync(cancellationToken);

        var cores = await _dbContext.Cores
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderBy(x => x.Nome)
            .Select(x => new PieceCatalogOptionResponse(x.Id, x.Nome))
            .ToListAsync(cancellationToken);

        var fornecedores = await (
                from pessoaLoja in _dbContext.PessoaLojas
                join pessoa in _dbContext.Pessoas on pessoaLoja.PessoaId equals pessoa.Id
                where pessoaLoja.LojaId == context.LojaId && pessoaLoja.EhFornecedor
                orderby pessoa.Nome
                select new PieceSupplierOptionResponse(
                    pessoa.Id,
                    pessoaLoja.Id,
                    pessoa.Nome,
                    pessoa.Documento,
                    pessoaLoja.PoliticaPadraoFimConsignacao,
                    pessoaLoja.StatusRelacao))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PieceWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            produtoNomes,
            marcas,
            tamanhos,
            cores,
            fornecedores,
            BuildPieceTypeOptions(),
            BuildPieceStatusOptions(),
            BuildImageVisibilityOptions());
    }

    /// <summary>
    /// Lista as pecas da loja ativa aplicando busca rapida e filtros operacionais.
    /// </summary>
    public async Task<IReadOnlyList<PieceSummaryResponse>> ListarAsync(
        PieceListQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanViewPiecesAsync(context.UsuarioId, context.LojaId, cancellationToken);

        var piecesQuery =
            from peca in _dbContext.Pecas.AsNoTracking()
            join produto in _dbContext.ProdutoNomes.AsNoTracking() on peca.ProdutoNomeId equals produto.Id
            join marca in _dbContext.Marcas.AsNoTracking() on peca.MarcaId equals marca.Id
            join tamanho in _dbContext.Tamanhos.AsNoTracking() on peca.TamanhoId equals tamanho.Id
            join cor in _dbContext.Cores.AsNoTracking() on peca.CorId equals cor.Id
            join fornecedor in _dbContext.Pessoas.AsNoTracking() on peca.FornecedorPessoaId equals fornecedor.Id into fornecedorGroup
            from fornecedor in fornecedorGroup.DefaultIfEmpty()
            join condicao in _dbContext.PecaCondicoesComerciais.AsNoTracking() on peca.Id equals condicao.PecaId into condicaoGroup
            from condicao in condicaoGroup.DefaultIfEmpty()
            where peca.LojaId == context.LojaId
            select new
            {
                Peca = peca,
                ProdutoNome = produto.Nome,
                Marca = marca.Nome,
                Tamanho = tamanho.Nome,
                Cor = cor.Nome,
                FornecedorNome = fornecedor != null ? fornecedor.Nome : null,
                DataFimConsignacao = condicao != null ? condicao.DataFimConsignacao : null,
            };

        if (!string.IsNullOrWhiteSpace(query.StatusPeca))
        {
            var normalizedStatus = query.StatusPeca.Trim().ToLowerInvariant();
            piecesQuery = piecesQuery.Where(x => x.Peca.StatusPeca == normalizedStatus);
        }

        if (query.ProdutoNomeId.HasValue)
        {
            piecesQuery = piecesQuery.Where(x => x.Peca.ProdutoNomeId == query.ProdutoNomeId.Value);
        }

        if (query.MarcaId.HasValue)
        {
            piecesQuery = piecesQuery.Where(x => x.Peca.MarcaId == query.MarcaId.Value);
        }

        if (query.FornecedorPessoaId.HasValue)
        {
            piecesQuery = piecesQuery.Where(x => x.Peca.FornecedorPessoaId == query.FornecedorPessoaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.CodigoBarras))
        {
            var barcode = query.CodigoBarras.Trim();
            piecesQuery = piecesQuery.Where(x => x.Peca.CodigoBarras == barcode);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            piecesQuery = piecesQuery.Where(x =>
                x.Peca.CodigoInterno.ToLower().Contains(term) ||
                x.Peca.CodigoBarras.ToLower().Contains(term) ||
                x.ProdutoNome.ToLower().Contains(term) ||
                x.Marca.ToLower().Contains(term) ||
                x.Cor.ToLower().Contains(term) ||
                x.Tamanho.ToLower().Contains(term) ||
                (x.FornecedorNome ?? string.Empty).ToLower().Contains(term) ||
                x.Peca.LocalizacaoFisica.ToLower().Contains(term) ||
                x.Peca.Descricao.ToLower().Contains(term));
        }

        var items = await piecesQuery
            .OrderByDescending(x => x.Peca.DataEntrada)
            .ThenByDescending(x => x.Peca.CriadoEm)
            .ToListAsync(cancellationToken);

        return items
            .Select(item => new PieceSummaryResponse(
                item.Peca.Id,
                item.Peca.CodigoInterno,
                item.Peca.CodigoBarras,
                item.Peca.TipoPeca,
                item.Peca.StatusPeca,
                item.Peca.ProdutoNomeId,
                item.ProdutoNome,
                item.Peca.MarcaId,
                item.Marca,
                item.Peca.TamanhoId,
                item.Tamanho,
                item.Peca.CorId,
                item.Cor,
                item.Peca.FornecedorPessoaId,
                item.FornecedorNome,
                item.Peca.DataEntrada,
                item.Peca.QuantidadeAtual,
                item.Peca.PrecoVendaAtual,
                item.Peca.LocalizacaoFisica,
                item.DataFimConsignacao))
            .ToArray();
    }

    /// <summary>
    /// Carrega o detalhe completo de uma peca da loja ativa.
    /// </summary>
    public async Task<PieceDetailResponse> ObterDetalheAsync(Guid pecaId, CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanViewPiecesAsync(context.UsuarioId, context.LojaId, cancellationToken);

        var detail = await LoadPieceDetailAsync(context.LojaId, pecaId, cancellationToken);
        return detail ?? throw new InvalidOperationException("Peca nao encontrada na loja ativa.");
    }

    /// <summary>
    /// Cria uma nova peca com snapshot comercial e entrada inicial no estoque.
    /// </summary>
    public async Task<PieceDetailResponse> CriarAsync(CreatePieceRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanManagePiecesAsync(context.UsuarioId, context.LojaId, cancellationToken);

        var normalizedType = PieceValues.NormalizePieceType(request.TipoPeca);
        ValidateCreateRequest(request, normalizedType);

        await EnsureCatalogReferencesAsync(
            context.LojaId,
            request.ProdutoNomeId,
            request.MarcaId,
            request.TamanhoId,
            request.CorId,
            cancellationToken);

        var supplier = await LoadSupplierAsync(
            context.LojaId,
            request.FornecedorPessoaId,
            normalizedType == PieceValues.PieceTypes.Consignada,
            cancellationToken);

        var dataEntrada = request.DataEntrada ?? DateTimeOffset.UtcNow;
        var codigoInterno = await GenerateInternalCodeAsync(context.LojaId, cancellationToken);

        var peca = new Peca
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            FornecedorPessoaId = supplier?.Pessoa.Id,
            TipoPeca = normalizedType,
            CodigoInterno = codigoInterno,
            CodigoBarras = request.CodigoBarras.Trim(),
            ProdutoNomeId = request.ProdutoNomeId,
            MarcaId = request.MarcaId,
            TamanhoId = request.TamanhoId,
            CorId = request.CorId,
            Descricao = request.Descricao.Trim(),
            Observacoes = request.Observacoes.Trim(),
            DataEntrada = dataEntrada,
            QuantidadeInicial = request.QuantidadeInicial,
            QuantidadeAtual = request.QuantidadeInicial,
            PrecoVendaAtual = request.PrecoVendaAtual,
            CustoUnitario = request.CustoUnitario,
            StatusPeca = PieceValues.PieceStatuses.Disponivel,
            LocalizacaoFisica = request.LocalizacaoFisica.Trim(),
            ResponsavelCadastroUsuarioId = context.UsuarioId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        var regraEfetiva = await ResolveEffectiveRuleAsync(
            context.LojaId,
            supplier?.PessoaLoja.Id,
            request.RegraManual,
            cancellationToken);

        var condicao = BuildCommercialCondition(
            peca.Id,
            regraEfetiva,
            supplier?.PessoaLoja,
            normalizedType,
            dataEntrada,
            context.UsuarioId);

        var entrada = new MovimentacaoEstoque
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            PecaId = peca.Id,
            TipoMovimentacao = PieceValues.StockMovementTypes.Entrada,
            Quantidade = request.QuantidadeInicial,
            SaldoAnterior = 0,
            SaldoPosterior = request.QuantidadeInicial,
            OrigemTipo = PieceValues.StockOrigins.Peca,
            OrigemId = peca.Id,
            Motivo = "Cadastro inicial da peca.",
            MovimentadoEm = DateTimeOffset.UtcNow,
            MovimentadoPorUsuarioId = context.UsuarioId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        var compraObrigacao = BuildPurchaseObligation(
            peca,
            supplier?.Pessoa,
            context.LojaId,
            request.QuantidadeInicial,
            request.CustoUnitario,
            context.UsuarioId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.Pecas.Add(peca);
        _dbContext.PecaCondicoesComerciais.Add(condicao);
        _dbContext.MovimentacoesEstoque.Add(entrada);
        if (compraObrigacao is not null)
        {
            _dbContext.ObrigacoesFornecedor.Add(compraObrigacao);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "peca",
            peca.Id,
            "criada",
            null,
            SnapshotPiece(peca),
            cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "peca_condicao_comercial",
            condicao.Id,
            "criada",
            null,
            SnapshotCommercialCondition(condicao),
            cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "movimentacao_estoque",
            entrada.Id,
            "criada",
            null,
            SnapshotStockMovement(entrada),
            cancellationToken);

        if (compraObrigacao is not null)
        {
            await _auditService.RegistrarAuditoriaAsync(
                context.LojaId,
                "obrigacao_fornecedor",
                compraObrigacao.Id,
                "criada",
                null,
                new
                {
                    compraObrigacao.Id,
                    compraObrigacao.PessoaId,
                    compraObrigacao.PecaId,
                    compraObrigacao.TipoObrigacao,
                    compraObrigacao.StatusObrigacao,
                    compraObrigacao.ValorOriginal,
                    compraObrigacao.ValorEmAberto,
                    compraObrigacao.DataGeracao,
                },
                cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return await ObterDetalheAsync(peca.Id, cancellationToken);
    }

    /// <summary>
    /// Atualiza os dados cadastrais da peca sem alterar o saldo atual de estoque.
    /// </summary>
    public async Task<PieceDetailResponse> AtualizarAsync(Guid pecaId, UpdatePieceRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanManagePiecesAsync(context.UsuarioId, context.LojaId, cancellationToken);

        var normalizedType = PieceValues.NormalizePieceType(request.TipoPeca);
        ValidateUpdateRequest(request, normalizedType);

        var peca = await _dbContext.Pecas
            .FirstOrDefaultAsync(x => x.Id == pecaId && x.LojaId == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Peca nao encontrada na loja ativa.");

        await EnsureCatalogReferencesAsync(
            context.LojaId,
            request.ProdutoNomeId,
            request.MarcaId,
            request.TamanhoId,
            request.CorId,
            cancellationToken);

        var supplier = await LoadSupplierAsync(
            context.LojaId,
            request.FornecedorPessoaId,
            normalizedType == PieceValues.PieceTypes.Consignada,
            cancellationToken);

        var beforePiece = SnapshotPiece(peca);

        peca.FornecedorPessoaId = supplier?.Pessoa.Id;
        peca.TipoPeca = normalizedType;
        peca.CodigoBarras = request.CodigoBarras.Trim();
        peca.ProdutoNomeId = request.ProdutoNomeId;
        peca.MarcaId = request.MarcaId;
        peca.TamanhoId = request.TamanhoId;
        peca.CorId = request.CorId;
        peca.Descricao = request.Descricao.Trim();
        peca.Observacoes = request.Observacoes.Trim();
        peca.DataEntrada = request.DataEntrada ?? peca.DataEntrada;
        peca.PrecoVendaAtual = request.PrecoVendaAtual;
        peca.CustoUnitario = request.CustoUnitario;
        peca.LocalizacaoFisica = request.LocalizacaoFisica.Trim();
        TouchEntity(peca, context.UsuarioId);

        var condicao = await _dbContext.PecaCondicoesComerciais
            .FirstOrDefaultAsync(x => x.PecaId == peca.Id, cancellationToken);

        var regraEfetiva = await ResolveEffectiveRuleAsync(
            context.LojaId,
            supplier?.PessoaLoja.Id,
            request.RegraManual,
            cancellationToken);

        if (condicao is null)
        {
            condicao = BuildCommercialCondition(
                peca.Id,
                regraEfetiva,
                supplier?.PessoaLoja,
                normalizedType,
                peca.DataEntrada,
                context.UsuarioId);

            _dbContext.PecaCondicoesComerciais.Add(condicao);
        }

        var beforeCondition = SnapshotCommercialCondition(condicao);

        ApplyCommercialCondition(
            condicao,
            regraEfetiva,
            supplier?.PessoaLoja,
            normalizedType,
            peca.DataEntrada,
            context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "peca",
            peca.Id,
            "atualizada",
            beforePiece,
            SnapshotPiece(peca),
            cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "peca_condicao_comercial",
            condicao.Id,
            "atualizada",
            beforeCondition,
            SnapshotCommercialCondition(condicao),
            cancellationToken);

        return await ObterDetalheAsync(peca.Id, cancellationToken);
    }

    /// <summary>
    /// Vincula uma imagem ja armazenada a peca informada.
    /// </summary>
    public async Task<PieceImageResponse> AdicionarImagemAsync(Guid pecaId, RegisterPieceImageRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanManagePiecesAsync(context.UsuarioId, context.LojaId, cancellationToken);

        var peca = await EnsurePieceFromActiveStoreAsync(pecaId, context.LojaId, cancellationToken);
        var normalizedVisibility = PieceValues.NormalizeImageVisibility(request.TipoVisibilidade);
        var url = NormalizeRequiredText(request.UrlArquivo, "Informe a URL da imagem.");
        var nextOrder = request.Ordem > 0
            ? request.Ordem
            : await ResolveNextImageOrderAsync(peca.Id, cancellationToken);

        var imagem = new PecaImagem
        {
            Id = Guid.NewGuid(),
            PecaId = peca.Id,
            UrlArquivo = url,
            Ordem = nextOrder,
            TipoVisibilidade = normalizedVisibility,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.PecaImagens.Add(imagem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "peca_imagem",
            imagem.Id,
            "criada",
            null,
            SnapshotImage(imagem),
            cancellationToken);

        return MapImage(imagem);
    }

    /// <summary>
    /// Atualiza a ordem e a visibilidade de uma imagem vinculada a peca.
    /// </summary>
    public async Task<PieceImageResponse> AtualizarImagemAsync(Guid pecaId, Guid imagemId, UpdatePieceImageRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanManagePiecesAsync(context.UsuarioId, context.LojaId, cancellationToken);

        await EnsurePieceFromActiveStoreAsync(pecaId, context.LojaId, cancellationToken);

        var imagem = await (
                from pecaImagem in _dbContext.PecaImagens
                join peca in _dbContext.Pecas on pecaImagem.PecaId equals peca.Id
                where pecaImagem.Id == imagemId && pecaImagem.PecaId == pecaId && peca.LojaId == context.LojaId
                select pecaImagem)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Imagem da peca nao encontrada.");

        var before = SnapshotImage(imagem);

        imagem.Ordem = request.Ordem > 0 ? request.Ordem : imagem.Ordem;
        imagem.TipoVisibilidade = PieceValues.NormalizeImageVisibility(request.TipoVisibilidade);
        TouchEntity(imagem, context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "peca_imagem",
            imagem.Id,
            "atualizada",
            before,
            SnapshotImage(imagem),
            cancellationToken);

        return MapImage(imagem);
    }

    /// <summary>
    /// Remove o vinculo da imagem com a peca informada.
    /// </summary>
    public async Task<PieceImageResponse> RemoverImagemAsync(Guid pecaId, Guid imagemId, CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanManagePiecesAsync(context.UsuarioId, context.LojaId, cancellationToken);

        await EnsurePieceFromActiveStoreAsync(pecaId, context.LojaId, cancellationToken);

        var imagem = await (
                from pecaImagem in _dbContext.PecaImagens
                join peca in _dbContext.Pecas on pecaImagem.PecaId equals peca.Id
                where pecaImagem.Id == imagemId && pecaImagem.PecaId == pecaId && peca.LojaId == context.LojaId
                select pecaImagem)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Imagem da peca nao encontrada.");

        var response = MapImage(imagem);
        var before = SnapshotImage(imagem);

        _dbContext.PecaImagens.Remove(imagem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "peca_imagem",
            imagem.Id,
            "removida",
            before,
            null,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Carrega o detalhe completo da peca com nomes auxiliares, condicao comercial e imagens.
    /// </summary>
    private async Task<PieceDetailResponse?> LoadPieceDetailAsync(Guid lojaId, Guid pecaId, CancellationToken cancellationToken)
    {
        var item = await (
                from peca in _dbContext.Pecas
                join produto in _dbContext.ProdutoNomes on peca.ProdutoNomeId equals produto.Id
                join marca in _dbContext.Marcas on peca.MarcaId equals marca.Id
                join tamanho in _dbContext.Tamanhos on peca.TamanhoId equals tamanho.Id
                join cor in _dbContext.Cores on peca.CorId equals cor.Id
                join fornecedor in _dbContext.Pessoas on peca.FornecedorPessoaId equals fornecedor.Id into fornecedorGroup
                from fornecedor in fornecedorGroup.DefaultIfEmpty()
                where peca.LojaId == lojaId && peca.Id == pecaId
                select new
                {
                    Peca = peca,
                    ProdutoNome = produto.Nome,
                    Marca = marca.Nome,
                    Tamanho = tamanho.Nome,
                    Cor = cor.Nome,
                    FornecedorNome = fornecedor != null ? fornecedor.Nome : null,
                })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var condicao = await _dbContext.PecaCondicoesComerciais
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PecaId == pecaId, cancellationToken)
            ?? throw new InvalidOperationException("Condicao comercial da peca nao encontrada.");

        var imagens = await _dbContext.PecaImagens
            .AsNoTracking()
            .Where(x => x.PecaId == pecaId)
            .OrderBy(x => x.Ordem)
            .ThenBy(x => x.CriadoEm)
            .ToListAsync(cancellationToken);

        return new PieceDetailResponse(
            item.Peca.Id,
            item.Peca.LojaId,
            item.Peca.CodigoInterno,
            item.Peca.CodigoBarras,
            item.Peca.TipoPeca,
            item.Peca.StatusPeca,
            item.Peca.ProdutoNomeId,
            item.ProdutoNome,
            item.Peca.MarcaId,
            item.Marca,
            item.Peca.TamanhoId,
            item.Tamanho,
            item.Peca.CorId,
            item.Cor,
            item.Peca.FornecedorPessoaId,
            item.FornecedorNome,
            item.Peca.Descricao,
            item.Peca.Observacoes,
            item.Peca.DataEntrada,
            item.Peca.QuantidadeInicial,
            item.Peca.QuantidadeAtual,
            item.Peca.PrecoVendaAtual,
            item.Peca.CustoUnitario,
            item.Peca.LocalizacaoFisica,
            item.Peca.ResponsavelCadastroUsuarioId,
            MapCommercialCondition(condicao),
            imagens.Select(MapImage).ToArray());
    }

    /// <summary>
    /// Garante usuario autenticado, loja ativa e vinculo valido no contexto atual.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureStoreContextAsync(CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para continuar.");

        var hasActiveMembership = await _dbContext.UsuarioLojas.AnyAsync(
            x => x.UsuarioId == usuarioId &&
                 x.LojaId == lojaId &&
                 x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                 (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
            cancellationToken);

        if (!hasActiveMembership)
        {
            throw new InvalidOperationException("Voce nao possui acesso a loja ativa informada.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Exige permissao de visualizacao ou cadastro no modulo de pecas.
    /// </summary>
    private async Task EnsureCanViewPiecesAsync(Guid usuarioId, Guid lojaId, CancellationToken cancellationToken)
    {
        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.PecasVisualizar, AccessPermissionCodes.PecasCadastrar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao tem acesso para visualizar pecas na loja ativa.");
        }
    }

    /// <summary>
    /// Exige permissao de cadastro e edicao no modulo de pecas.
    /// </summary>
    private async Task EnsureCanManagePiecesAsync(Guid usuarioId, Guid lojaId, CancellationToken cancellationToken)
    {
        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.PecasCadastrar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao tem acesso para gerenciar pecas na loja ativa.");
        }
    }

    /// <summary>
    /// Verifica se o usuario possui ao menos uma das permissoes informadas na loja ativa.
    /// </summary>
    private async Task<bool> HasPermissionAsync(Guid usuarioId, Guid lojaId, IReadOnlyCollection<string> permissionCodes, CancellationToken cancellationToken)
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
    /// Garante que os cadastros auxiliares informados pertencem a loja ativa.
    /// </summary>
    private async Task EnsureCatalogReferencesAsync(Guid lojaId, Guid produtoNomeId, Guid marcaId, Guid tamanhoId, Guid corId, CancellationToken cancellationToken)
    {
        var validProduct = await _dbContext.ProdutoNomes.AnyAsync(x => x.Id == produtoNomeId && x.LojaId == lojaId, cancellationToken);
        var validBrand = await _dbContext.Marcas.AnyAsync(x => x.Id == marcaId && x.LojaId == lojaId, cancellationToken);
        var validSize = await _dbContext.Tamanhos.AnyAsync(x => x.Id == tamanhoId && x.LojaId == lojaId, cancellationToken);
        var validColor = await _dbContext.Cores.AnyAsync(x => x.Id == corId && x.LojaId == lojaId, cancellationToken);

        if (!validProduct || !validBrand || !validSize || !validColor)
        {
            throw new InvalidOperationException("Informe apenas cadastros auxiliares pertencentes a loja ativa.");
        }
    }

    /// <summary>
    /// Carrega a pessoa fornecedora e a relacao dela com a loja ativa quando informada.
    /// </summary>
    private async Task<(Pessoa Pessoa, PessoaLoja PessoaLoja)?> LoadSupplierAsync(Guid lojaId, Guid? fornecedorPessoaId, bool required, CancellationToken cancellationToken)
    {
        if (!fornecedorPessoaId.HasValue)
        {
            if (required)
            {
                throw new InvalidOperationException("Selecione o fornecedor da peca consignada.");
            }

            return null;
        }

        var supplier = await (
                from pessoaLoja in _dbContext.PessoaLojas
                join pessoa in _dbContext.Pessoas on pessoaLoja.PessoaId equals pessoa.Id
                where pessoaLoja.LojaId == lojaId
                where pessoaLoja.PessoaId == fornecedorPessoaId.Value
                where pessoaLoja.EhFornecedor
                select new
                {
                    Pessoa = pessoa,
                    PessoaLoja = pessoaLoja,
                })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Fornecedor nao encontrado na loja ativa.");

        return (supplier.Pessoa, supplier.PessoaLoja);
    }

    /// <summary>
    /// Resolve a regra comercial efetiva a ser congelada na peca.
    /// </summary>
    private async Task<EffectiveCommercialRuleResponse> ResolveEffectiveRuleAsync(Guid lojaId, Guid? pessoaLojaId, ManualPieceCommercialRuleRequest? manualRule, CancellationToken cancellationToken)
    {
        ManualCommercialRuleInput? manualInput = null;
        if (manualRule is not null)
        {
            ValidateManualRule(manualRule);
            manualInput = new ManualCommercialRuleInput(
                manualRule.PercentualRepasseDinheiro,
                manualRule.PercentualRepasseCredito,
                manualRule.PermitePagamentoMisto,
                manualRule.TempoMaximoExposicaoDias,
                manualRule.PoliticaDesconto
                    .OrderBy(x => x.DiasMinimos)
                    .Select(x => new CommercialDiscountBandResponse(x.DiasMinimos, x.PercentualDesconto))
                    .ToArray());
        }

        return await _commercialRuleResolverService.ResolverAsync(lojaId, pessoaLojaId, manualInput, cancellationToken);
    }

    /// <summary>
    /// Constroi a condicao comercial da peca no momento da entrada.
    /// </summary>
    private static PecaCondicaoComercial BuildCommercialCondition(Guid pecaId, EffectiveCommercialRuleResponse regra, PessoaLoja? fornecedorRelacao, string tipoPeca, DateTimeOffset dataEntrada, Guid usuarioId)
    {
        var entity = new PecaCondicaoComercial
        {
            Id = Guid.NewGuid(),
            PecaId = pecaId,
            CriadoPorUsuarioId = usuarioId,
        };

        ApplyCommercialCondition(entity, regra, fornecedorRelacao, tipoPeca, dataEntrada, usuarioId);
        return entity;
    }

    /// <summary>
    /// Atualiza o snapshot comercial efetivo da peca.
    /// </summary>
    private static void ApplyCommercialCondition(PecaCondicaoComercial entity, EffectiveCommercialRuleResponse regra, PessoaLoja? fornecedorRelacao, string tipoPeca, DateTimeOffset dataEntrada, Guid usuarioId)
    {
        entity.OrigemRegra = regra.OrigemRegra;
        entity.PercentualRepasseDinheiro = regra.PercentualRepasseDinheiro;
        entity.PercentualRepasseCredito = regra.PercentualRepasseCredito;
        entity.PermitePagamentoMisto = regra.PermitePagamentoMisto;
        entity.TempoMaximoExposicaoDias = regra.TempoMaximoExposicaoDias;
        entity.PoliticaDescontoJson = CommercialRulePolicySerializer.Serialize(regra.PoliticaDesconto);
        entity.DataInicioConsignacao = tipoPeca == PieceValues.PieceTypes.Consignada ? dataEntrada : null;
        entity.DataFimConsignacao = tipoPeca == PieceValues.PieceTypes.Consignada ? dataEntrada.AddDays(regra.TempoMaximoExposicaoDias) : null;
        entity.DestinoPadraoFimConsignacao = tipoPeca == PieceValues.PieceTypes.Consignada ? fornecedorRelacao?.PoliticaPadraoFimConsignacao : null;
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Gera o proximo codigo interno unico da loja ativa.
    /// </summary>
    private async Task<string> GenerateInternalCodeAsync(Guid lojaId, CancellationToken cancellationToken)
    {
        var codes = await _dbContext.Pecas
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Select(x => x.CodigoInterno)
            .ToListAsync(cancellationToken);

        var max = 0;
        foreach (var code in codes)
        {
            const string prefix = "PEC-";
            if (!code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (int.TryParse(code[prefix.Length..], out var numeric) && numeric > max)
            {
                max = numeric;
            }
        }

        return $"PEC-{max + 1:000000}";
    }

    /// <summary>
    /// Recupera a peca da loja ativa antes de operacoes que exigem escopo estrito.
    /// </summary>
    private async Task<Peca> EnsurePieceFromActiveStoreAsync(Guid pecaId, Guid lojaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Pecas
            .FirstOrDefaultAsync(x => x.Id == pecaId && x.LojaId == lojaId, cancellationToken)
            ?? throw new InvalidOperationException("Peca nao encontrada na loja ativa.");
    }

    /// <summary>
    /// Resolve a proxima ordem disponivel para uma nova imagem da peca.
    /// </summary>
    private async Task<int> ResolveNextImageOrderAsync(Guid pecaId, CancellationToken cancellationToken)
    {
        var currentOrder = await _dbContext.PecaImagens
            .Where(x => x.PecaId == pecaId)
            .Select(x => (int?)x.Ordem)
            .MaxAsync(cancellationToken);

        return (currentOrder ?? 0) + 1;
    }

    /// <summary>
    /// Valida o payload de criacao da peca.
    /// </summary>
    private static void ValidateCreateRequest(CreatePieceRequest request, string normalizedType)
    {
        ValidateCommonPiecePayload(normalizedType, request.CodigoBarras, request.PrecoVendaAtual, request.CustoUnitario, request.LocalizacaoFisica);

        if (request.QuantidadeInicial <= 0)
        {
            throw new InvalidOperationException("Informe uma quantidade inicial maior que zero.");
        }
    }

    /// <summary>
    /// Valida o payload de atualizacao da peca.
    /// </summary>
    private static void ValidateUpdateRequest(UpdatePieceRequest request, string normalizedType)
    {
        ValidateCommonPiecePayload(normalizedType, request.CodigoBarras, request.PrecoVendaAtual, request.CustoUnitario, request.LocalizacaoFisica);
    }

    /// <summary>
    /// Valida os campos comuns de criacao e edicao da peca.
    /// </summary>
    private static void ValidateCommonPiecePayload(string normalizedType, string codigoBarras, decimal precoVendaAtual, decimal? custoUnitario, string localizacaoFisica)
    {
        _ = normalizedType;
        _ = codigoBarras.Trim();

        if (precoVendaAtual <= 0)
        {
            throw new InvalidOperationException("Informe um preco de venda maior que zero.");
        }

        if (custoUnitario.HasValue && custoUnitario.Value < 0)
        {
            throw new InvalidOperationException("O custo unitario nao pode ser negativo.");
        }

        _ = NormalizeRequiredText(localizacaoFisica, "Informe a localizacao fisica da peca.");
    }

    /// <summary>
    /// Valida a regra manual da peca antes da resolucao efetiva.
    /// </summary>
    private static void ValidateManualRule(ManualPieceCommercialRuleRequest request)
    {
        if (request.PercentualRepasseDinheiro < 0 || request.PercentualRepasseDinheiro > 100)
        {
            throw new InvalidOperationException("Informe um percentual manual de dinheiro entre 0 e 100.");
        }

        if (request.PercentualRepasseCredito < 0 || request.PercentualRepasseCredito > 100)
        {
            throw new InvalidOperationException("Informe um percentual manual de credito entre 0 e 100.");
        }

        if (request.TempoMaximoExposicaoDias <= 0)
        {
            throw new InvalidOperationException("Informe um prazo manual maior que zero.");
        }

        var days = new HashSet<int>();
        foreach (var band in request.PoliticaDesconto)
        {
            if (band.DiasMinimos <= 0)
            {
                throw new InvalidOperationException("As faixas de desconto manuais exigem dias minimos maiores que zero.");
            }

            if (!days.Add(band.DiasMinimos))
            {
                throw new InvalidOperationException("Nao repita a mesma faixa de dias na regra manual.");
            }

            if (band.PercentualDesconto < 0 || band.PercentualDesconto > 100)
            {
                throw new InvalidOperationException("Cada desconto manual deve ficar entre 0 e 100.");
            }
        }
    }

    /// <summary>
    /// Atualiza os metadados comuns das entidades auditaveis.
    /// </summary>
    private static void TouchEntity(AuditEntityBase entity, Guid usuarioId)
    {
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Gera a obrigacao imediata de compra para pecas fixas ou em lote quando houver custo e fornecedor.
    /// </summary>
    private static ObrigacaoFornecedor? BuildPurchaseObligation(
        Peca peca,
        Pessoa? fornecedor,
        Guid lojaId,
        int quantidadeInicial,
        decimal? custoUnitario,
        Guid usuarioId)
    {
        if (fornecedor is null || !custoUnitario.HasValue || custoUnitario.Value <= 0m)
        {
            return null;
        }

        string? tipoObrigacao = peca.TipoPeca switch
        {
            PieceValues.PieceTypes.Fixa => SupplierPaymentValues.ObligationTypes.CompraPecaFixa,
            PieceValues.PieceTypes.Lote => SupplierPaymentValues.ObligationTypes.CompraPecaLote,
            _ => null,
        };

        if (tipoObrigacao is null)
        {
            return null;
        }

        var total = Math.Round(custoUnitario.Value * quantidadeInicial, 2, MidpointRounding.AwayFromZero);
        return new ObrigacaoFornecedor
        {
            Id = Guid.NewGuid(),
            LojaId = lojaId,
            PessoaId = fornecedor.Id,
            PecaId = peca.Id,
            TipoObrigacao = tipoObrigacao,
            DataGeracao = peca.DataEntrada,
            DataVencimento = peca.DataEntrada,
            ValorOriginal = total,
            ValorEmAberto = total,
            StatusObrigacao = SupplierPaymentValues.ObligationStatuses.Pendente,
            Observacoes = $"Obrigacao gerada automaticamente pela entrada da peca {peca.CodigoInterno}.",
            CriadoPorUsuarioId = usuarioId,
        };
    }

    /// <summary>
    /// Normaliza um texto obrigatorio.
    /// </summary>
    private static string NormalizeRequiredText(string value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    /// <summary>
    /// Mapeia a condicao comercial persistida para o contrato da API.
    /// </summary>
    private static PieceCommercialConditionResponse MapCommercialCondition(PecaCondicaoComercial entity)
    {
        return new PieceCommercialConditionResponse(
            entity.Id,
            entity.OrigemRegra,
            entity.PercentualRepasseDinheiro,
            entity.PercentualRepasseCredito,
            entity.PermitePagamentoMisto,
            entity.TempoMaximoExposicaoDias,
            CommercialRulePolicySerializer.Deserialize(entity.PoliticaDescontoJson),
            entity.DataInicioConsignacao,
            entity.DataFimConsignacao,
            entity.DestinoPadraoFimConsignacao);
    }

    /// <summary>
    /// Mapeia a imagem persistida para o contrato da API.
    /// </summary>
    private static PieceImageResponse MapImage(PecaImagem entity)
    {
        return new PieceImageResponse(entity.Id, entity.UrlArquivo, entity.Ordem, entity.TipoVisibilidade);
    }

    /// <summary>
    /// Expone os tipos de peca para o frontend.
    /// </summary>
    private static IReadOnlyList<PieceOptionResponse> BuildPieceTypeOptions()
    {
        return
        [
            new(PieceValues.PieceTypes.Consignada, "Consignada"),
            new(PieceValues.PieceTypes.Fixa, "Fixa"),
            new(PieceValues.PieceTypes.Lote, "Lote"),
        ];
    }

    /// <summary>
    /// Expone os status operacionais de peca.
    /// </summary>
    private static IReadOnlyList<PieceOptionResponse> BuildPieceStatusOptions()
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
    /// Expone as visibilidades permitidas para imagens da peca.
    /// </summary>
    private static IReadOnlyList<PieceOptionResponse> BuildImageVisibilityOptions()
    {
        return
        [
            new(PieceValues.ImageVisibility.Interna, "Interna"),
            new(PieceValues.ImageVisibility.Externa, "Externa"),
        ];
    }

    /// <summary>
    /// Resume os dados principais da peca para auditoria.
    /// </summary>
    private static object SnapshotPiece(Peca entity)
    {
        return new
        {
            entity.LojaId,
            entity.FornecedorPessoaId,
            entity.TipoPeca,
            entity.CodigoInterno,
            entity.CodigoBarras,
            entity.ProdutoNomeId,
            entity.MarcaId,
            entity.TamanhoId,
            entity.CorId,
            entity.DataEntrada,
            entity.QuantidadeInicial,
            entity.QuantidadeAtual,
            entity.PrecoVendaAtual,
            entity.CustoUnitario,
            entity.StatusPeca,
            entity.LocalizacaoFisica,
        };
    }

    /// <summary>
    /// Resume a condicao comercial congelada da peca.
    /// </summary>
    private static object SnapshotCommercialCondition(PecaCondicaoComercial entity)
    {
        return new
        {
            entity.PecaId,
            entity.OrigemRegra,
            entity.PercentualRepasseDinheiro,
            entity.PercentualRepasseCredito,
            entity.PermitePagamentoMisto,
            entity.TempoMaximoExposicaoDias,
            PoliticaDesconto = CommercialRulePolicySerializer.Deserialize(entity.PoliticaDescontoJson),
            entity.DataInicioConsignacao,
            entity.DataFimConsignacao,
            entity.DestinoPadraoFimConsignacao,
        };
    }

    /// <summary>
    /// Resume uma imagem da peca para auditoria.
    /// </summary>
    private static object SnapshotImage(PecaImagem entity)
    {
        return new
        {
            entity.PecaId,
            entity.UrlArquivo,
            entity.Ordem,
            entity.TipoVisibilidade,
        };
    }

    /// <summary>
    /// Resume uma movimentacao de estoque para auditoria.
    /// </summary>
    private static object SnapshotStockMovement(MovimentacaoEstoque entity)
    {
        return new
        {
            entity.LojaId,
            entity.PecaId,
            entity.TipoMovimentacao,
            entity.Quantidade,
            entity.SaldoAnterior,
            entity.SaldoPosterior,
            entity.OrigemTipo,
            entity.OrigemId,
            entity.Motivo,
        };
    }
}
