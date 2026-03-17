using Renova.Services.Features.Stores.Contracts;

namespace Renova.Services.Features.Stores.Abstractions;

// Contrato principal do modulo de lojas e configuracoes operacionais.
public interface IStoreService
{
    /// <summary>
    /// Lista as lojas que o usuario autenticado pode visualizar no contexto consolidado.
    /// </summary>
    Task<IReadOnlyList<StoreResponse>> ListarAcessiveisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma nova loja com configuracao inicial e vincula o criador como responsavel.
    /// </summary>
    Task<StoreResponse> CriarAsync(CreateStoreRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os dados principais de uma loja acessivel ao usuario.
    /// </summary>
    Task<StoreResponse> AtualizarAsync(Guid lojaId, UpdateStoreRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os parametros operacionais e de impressao da loja.
    /// </summary>
    Task<StoreResponse> AtualizarConfiguracaoAsync(Guid lojaId, UpdateStoreConfigurationRequest request, CancellationToken cancellationToken = default);
}
