using Renova.Services.Features.Stores.Contracts;

namespace Renova.Services.Features.Stores.Abstractions;

// Contrato principal do modulo de lojas.
public interface IStoreService
{
    /// <summary>
    /// Lista as lojas que o usuario autenticado pode visualizar.
    /// </summary>
    Task<IReadOnlyList<StoreResponse>> ListarAcessiveisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma nova loja e vincula o criador como responsavel.
    /// </summary>
    Task<StoreResponse> CriarAsync(CreateStoreRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os dados principais de uma loja acessivel ao usuario.
    /// </summary>
    Task<StoreResponse> AtualizarAsync(Guid lojaId, UpdateStoreRequest request, CancellationToken cancellationToken = default);
}
