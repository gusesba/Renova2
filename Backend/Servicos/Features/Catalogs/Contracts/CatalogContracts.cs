namespace Renova.Services.Features.Catalogs.Contracts;

// Reune os contratos HTTP e de aplicacao do modulo 04 com tabelas auxiliares enxutas.
public sealed record CatalogWorkspaceResponse(
    Guid LojaAtivaId,
    string LojaAtivaNome,
    IReadOnlyList<ProductNameResponse> ProdutoNomes,
    IReadOnlyList<BrandResponse> Marcas,
    IReadOnlyList<SizeResponse> Tamanhos,
    IReadOnlyList<ColorResponse> Cores);

public sealed record CreateProductNameRequest(string Nome);

public sealed record UpdateProductNameRequest(string Nome);

public sealed record ProductNameResponse(Guid Id, string Nome);

public sealed record CreateBrandRequest(string Nome);

public sealed record UpdateBrandRequest(string Nome);

public sealed record BrandResponse(Guid Id, string Nome);

public sealed record CreateSizeRequest(string Nome);

public sealed record UpdateSizeRequest(string Nome);

public sealed record SizeResponse(Guid Id, string Nome);

public sealed record CreateColorRequest(string Nome);

public sealed record UpdateColorRequest(string Nome);

public sealed record ColorResponse(Guid Id, string Nome);
