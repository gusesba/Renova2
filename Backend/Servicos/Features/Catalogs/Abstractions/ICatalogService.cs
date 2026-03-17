using Renova.Services.Features.Catalogs.Contracts;

namespace Renova.Services.Features.Catalogs.Abstractions;

// Define os casos de uso do modulo 04 com tabelas auxiliares enxutas por loja.
public interface ICatalogService
{
    /// <summary>
    /// Carrega os cadastros auxiliares da loja ativa.
    /// </summary>
    Task<CatalogWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria um nome de produto na loja ativa.
    /// </summary>
    Task<ProductNameResponse> CriarProdutoNomeAsync(CreateProductNameRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um nome de produto existente.
    /// </summary>
    Task<ProductNameResponse> AtualizarProdutoNomeAsync(Guid produtoNomeId, UpdateProductNameRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma marca na loja ativa.
    /// </summary>
    Task<BrandResponse> CriarMarcaAsync(CreateBrandRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma marca existente.
    /// </summary>
    Task<BrandResponse> AtualizarMarcaAsync(Guid marcaId, UpdateBrandRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria um tamanho na loja ativa.
    /// </summary>
    Task<SizeResponse> CriarTamanhoAsync(CreateSizeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um tamanho existente.
    /// </summary>
    Task<SizeResponse> AtualizarTamanhoAsync(Guid tamanhoId, UpdateSizeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma cor na loja ativa.
    /// </summary>
    Task<ColorResponse> CriarCorAsync(CreateColorRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma cor existente.
    /// </summary>
    Task<ColorResponse> AtualizarCorAsync(Guid corId, UpdateColorRequest request, CancellationToken cancellationToken = default);
}
