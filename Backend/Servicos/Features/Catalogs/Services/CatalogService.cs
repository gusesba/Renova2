using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Catalogs.Abstractions;
using Renova.Services.Features.Catalogs.Contracts;

namespace Renova.Services.Features.Catalogs.Services;

// Implementa o modulo 04 com quatro tabelas auxiliares reduzidas por loja.
public sealed class CatalogService : ICatalogService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public CatalogService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega os cadastros auxiliares da loja ativa.
    /// </summary>
    public async Task<CatalogWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureCatalogContextAsync(cancellationToken);

        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var produtoNomes = await _dbContext.ProdutoNomes
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderBy(x => x.Nome)
            .Select(x => MapProdutoNome(x))
            .ToListAsync(cancellationToken);

        var marcas = await _dbContext.Marcas
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderBy(x => x.Nome)
            .Select(x => MapMarca(x))
            .ToListAsync(cancellationToken);

        var tamanhos = await _dbContext.Tamanhos
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderBy(x => x.Nome)
            .Select(x => MapTamanho(x))
            .ToListAsync(cancellationToken);

        var cores = await _dbContext.Cores
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderBy(x => x.Nome)
            .Select(x => MapCor(x))
            .ToListAsync(cancellationToken);

        return new CatalogWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            produtoNomes,
            marcas,
            tamanhos,
            cores);
    }

    /// <summary>
    /// Cria um nome de produto na loja ativa.
    /// </summary>
    public async Task<ProductNameResponse> CriarProdutoNomeAsync(CreateProductNameRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureCatalogContextAsync(cancellationToken);
        var nome = NormalizeRequiredText(request.Nome, "Informe o nome do produto.");
        await EnsureUniqueAsync(_dbContext.ProdutoNomes, context.LojaId, nome, null, cancellationToken);

        var entity = new ProdutoNome
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            Nome = nome,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.ProdutoNomes.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await RegisterCatalogAuditAsync(context.LojaId, "produto_nome", entity.Id, "criado", null, entity.Nome, cancellationToken);
        return MapProdutoNome(entity);
    }

    /// <summary>
    /// Atualiza um nome de produto existente.
    /// </summary>
    public async Task<ProductNameResponse> AtualizarProdutoNomeAsync(Guid produtoNomeId, UpdateProductNameRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureCatalogContextAsync(cancellationToken);
        var entity = await EnsureEntityFromActiveStoreAsync(
            _dbContext.ProdutoNomes,
            produtoNomeId,
            x => x.LojaId,
            "Nome de produto nao encontrado.",
            context.LojaId,
            cancellationToken);

        var nome = NormalizeRequiredText(request.Nome, "Informe o nome do produto.");
        await EnsureUniqueAsync(_dbContext.ProdutoNomes, context.LojaId, nome, entity.Id, cancellationToken);

        var before = SnapshotAuxiliary(entity.Nome);

        entity.Nome = nome;
        TouchEntity(entity, context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await RegisterCatalogAuditAsync(context.LojaId, "produto_nome", entity.Id, "atualizado", before, entity.Nome, cancellationToken);
        return MapProdutoNome(entity);
    }

    /// <summary>
    /// Cria uma marca na loja ativa.
    /// </summary>
    public async Task<BrandResponse> CriarMarcaAsync(CreateBrandRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureCatalogContextAsync(cancellationToken);
        var nome = NormalizeRequiredText(request.Nome, "Informe o nome da marca.");
        await EnsureUniqueAsync(_dbContext.Marcas, context.LojaId, nome, null, cancellationToken);

        var entity = new Marca
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            Nome = nome,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.Marcas.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await RegisterCatalogAuditAsync(context.LojaId, "marca", entity.Id, "criada", null, entity.Nome, cancellationToken);
        return MapMarca(entity);
    }

    /// <summary>
    /// Atualiza uma marca existente.
    /// </summary>
    public async Task<BrandResponse> AtualizarMarcaAsync(Guid marcaId, UpdateBrandRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureCatalogContextAsync(cancellationToken);
        var entity = await EnsureEntityFromActiveStoreAsync(
            _dbContext.Marcas,
            marcaId,
            x => x.LojaId,
            "Marca nao encontrada.",
            context.LojaId,
            cancellationToken);

        var nome = NormalizeRequiredText(request.Nome, "Informe o nome da marca.");
        await EnsureUniqueAsync(_dbContext.Marcas, context.LojaId, nome, entity.Id, cancellationToken);

        var before = SnapshotAuxiliary(entity.Nome);

        entity.Nome = nome;
        TouchEntity(entity, context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await RegisterCatalogAuditAsync(context.LojaId, "marca", entity.Id, "atualizada", before, entity.Nome, cancellationToken);
        return MapMarca(entity);
    }

    /// <summary>
    /// Cria um tamanho na loja ativa.
    /// </summary>
    public async Task<SizeResponse> CriarTamanhoAsync(CreateSizeRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureCatalogContextAsync(cancellationToken);
        var nome = NormalizeRequiredText(request.Nome, "Informe o nome do tamanho.");
        await EnsureUniqueAsync(_dbContext.Tamanhos, context.LojaId, nome, null, cancellationToken);

        var entity = new Tamanho
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            Nome = nome,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.Tamanhos.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await RegisterCatalogAuditAsync(context.LojaId, "tamanho", entity.Id, "criado", null, entity.Nome, cancellationToken);
        return MapTamanho(entity);
    }

    /// <summary>
    /// Atualiza um tamanho existente.
    /// </summary>
    public async Task<SizeResponse> AtualizarTamanhoAsync(Guid tamanhoId, UpdateSizeRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureCatalogContextAsync(cancellationToken);
        var entity = await EnsureEntityFromActiveStoreAsync(
            _dbContext.Tamanhos,
            tamanhoId,
            x => x.LojaId,
            "Tamanho nao encontrado.",
            context.LojaId,
            cancellationToken);

        var nome = NormalizeRequiredText(request.Nome, "Informe o nome do tamanho.");
        await EnsureUniqueAsync(_dbContext.Tamanhos, context.LojaId, nome, entity.Id, cancellationToken);

        var before = SnapshotAuxiliary(entity.Nome);

        entity.Nome = nome;
        TouchEntity(entity, context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await RegisterCatalogAuditAsync(context.LojaId, "tamanho", entity.Id, "atualizado", before, entity.Nome, cancellationToken);
        return MapTamanho(entity);
    }

    /// <summary>
    /// Cria uma cor na loja ativa.
    /// </summary>
    public async Task<ColorResponse> CriarCorAsync(CreateColorRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureCatalogContextAsync(cancellationToken);
        var nome = NormalizeRequiredText(request.Nome, "Informe o nome da cor.");
        await EnsureUniqueAsync(_dbContext.Cores, context.LojaId, nome, null, cancellationToken);

        var entity = new Cor
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            Nome = nome,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.Cores.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await RegisterCatalogAuditAsync(context.LojaId, "cor", entity.Id, "criada", null, entity.Nome, cancellationToken);
        return MapCor(entity);
    }

    /// <summary>
    /// Atualiza uma cor existente.
    /// </summary>
    public async Task<ColorResponse> AtualizarCorAsync(Guid corId, UpdateColorRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureCatalogContextAsync(cancellationToken);
        var entity = await EnsureEntityFromActiveStoreAsync(
            _dbContext.Cores,
            corId,
            x => x.LojaId,
            "Cor nao encontrada.",
            context.LojaId,
            cancellationToken);

        var nome = NormalizeRequiredText(request.Nome, "Informe o nome da cor.");
        await EnsureUniqueAsync(_dbContext.Cores, context.LojaId, nome, entity.Id, cancellationToken);

        var before = SnapshotAuxiliary(entity.Nome);

        entity.Nome = nome;
        TouchEntity(entity, context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await RegisterCatalogAuditAsync(context.LojaId, "cor", entity.Id, "atualizada", before, entity.Nome, cancellationToken);
        return MapCor(entity);
    }

    /// <summary>
    /// Garante usuario autenticado, loja ativa e permissao de catalogo no contexto corrente.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureCatalogContextAsync(CancellationToken cancellationToken)
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
            [AccessPermissionCodes.CatalogoGerenciar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao tem acesso para gerenciar catalogos na loja ativa.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Garante que a entidade pertence a loja ativa antes da edicao.
    /// </summary>
    private static async Task<TEntity> EnsureEntityFromActiveStoreAsync<TEntity>(
        DbSet<TEntity> dbSet,
        Guid entityId,
        Func<TEntity, Guid> storeIdAccessor,
        string notFoundMessage,
        Guid lojaId,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var entity = await dbSet.FirstOrDefaultAsync(
                         x => EF.Property<Guid>(x, "Id") == entityId,
                         cancellationToken)
                     ?? throw new InvalidOperationException(notFoundMessage);

        if (storeIdAccessor(entity) != lojaId)
        {
            throw new InvalidOperationException("Voce nao tem acesso ao cadastro informado na loja ativa.");
        }

        return entity;
    }

    /// <summary>
    /// Verifica se o usuario possui a permissao de catalogo na loja alvo.
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
    /// Garante unicidade de nome dentro da loja e da tabela auxiliar alvo.
    /// </summary>
    private static async Task EnsureUniqueAsync<TEntity>(
        DbSet<TEntity> dbSet,
        Guid lojaId,
        string nome,
        Guid? currentId,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var normalized = nome.ToLowerInvariant();
        var exists = await dbSet.AnyAsync(
            x => EF.Property<Guid>(x, "LojaId") == lojaId &&
                 EF.Property<Guid>(x, "Id") != currentId &&
                 EF.Property<string>(x, "Nome").ToLower() == normalized,
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ja existe um cadastro com esse nome na loja ativa.");
        }
    }

    /// <summary>
    /// Atualiza os metadados de auditoria comuns das tabelas auxiliares.
    /// </summary>
    private static void TouchEntity(AuditEntityBase entity, Guid usuarioId)
    {
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Registra a auditoria comum das tabelas auxiliares do modulo.
    /// </summary>
    private async Task RegisterCatalogAuditAsync(
        Guid lojaId,
        string entityName,
        Guid entityId,
        string action,
        object? before,
        object after,
        CancellationToken cancellationToken)
    {
        await _auditService.RegistrarAuditoriaAsync(
            lojaId,
            entityName,
            entityId,
            action,
            before,
            SnapshotAuxiliary(after.ToString() ?? string.Empty),
            cancellationToken);
    }

    /// <summary>
    /// Normaliza e valida um texto obrigatorio.
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
    /// Mapeia um nome de produto para o contrato da API.
    /// </summary>
    private static ProductNameResponse MapProdutoNome(ProdutoNome entity)
    {
        return new ProductNameResponse(entity.Id, entity.Nome);
    }

    /// <summary>
    /// Mapeia uma marca para o contrato da API.
    /// </summary>
    private static BrandResponse MapMarca(Marca entity)
    {
        return new BrandResponse(entity.Id, entity.Nome);
    }

    /// <summary>
    /// Mapeia um tamanho para o contrato da API.
    /// </summary>
    private static SizeResponse MapTamanho(Tamanho entity)
    {
        return new SizeResponse(entity.Id, entity.Nome);
    }

    /// <summary>
    /// Mapeia uma cor para o contrato da API.
    /// </summary>
    private static ColorResponse MapCor(Cor entity)
    {
        return new ColorResponse(entity.Id, entity.Nome);
    }

    /// <summary>
    /// Captura o estado relevante das tabelas auxiliares simplificadas.
    /// </summary>
    private static object SnapshotAuxiliary(string nome)
    {
        return new
        {
            Nome = nome,
        };
    }
}
