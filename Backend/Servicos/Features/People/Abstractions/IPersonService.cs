using Renova.Services.Features.People.Contracts;

namespace Renova.Services.Features.People.Abstractions;

// Define os casos de uso do modulo 03 para API e demais consumidores internos.
public interface IPersonService
{
    /// <summary>
    /// Lista as pessoas vinculadas a loja ativa do usuario.
    /// </summary>
    Task<IReadOnlyList<PersonSummaryResponse>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Carrega o detalhe completo de uma pessoa dentro da loja ativa.
    /// </summary>
    Task<PersonDetailResponse> ObterDetalheAsync(Guid pessoaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista usuarios que podem ser vinculados ao cadastro da pessoa.
    /// </summary>
    Task<IReadOnlyList<PersonUserOptionResponse>> ListarUsuariosVinculaveisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria o cadastro mestre da pessoa e o vinculo com a loja ativa.
    /// </summary>
    Task<PersonDetailResponse> CriarAsync(CreatePersonRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o cadastro mestre, o vinculo da loja e os dados bancarios.
    /// </summary>
    Task<PersonDetailResponse> AtualizarAsync(Guid pessoaId, UpdatePersonRequest request, CancellationToken cancellationToken = default);
}
