using Renova.Services.Features.Access.Contracts;

namespace Renova.Services.Features.Access.Abstractions;

// Representa o contrato principal do fluxo de autenticacao e sessao.
public interface IAccessAuthService
{
    /// <summary>
    /// Autentica um usuario e retorna token com contexto.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoga a sessao autenticada atual.
    /// </summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o contexto atualizado da sessao autenticada.
    /// </summary>
    Task<LoginContextResponse> ObterContextoAtualAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Troca a loja ativa usada na sessao atual.
    /// </summary>
    Task<LoginContextResponse> AlterarLojaAtivaAsync(SwitchActiveStoreRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solicita a emissao de um token de recuperacao de senha.
    /// </summary>
    Task<PasswordResetRequestResponse> SolicitarRecuperacaoAsync(PasswordResetRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Redefine a senha a partir de um token valido.
    /// </summary>
    Task RedefinirSenhaAsync(ConfirmPasswordResetRequest request, CancellationToken cancellationToken = default);
}
