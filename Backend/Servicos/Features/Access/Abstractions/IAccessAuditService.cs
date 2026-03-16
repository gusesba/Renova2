namespace Renova.Services.Features.Access.Abstractions;

// Representa o contrato de auditoria tecnica do modulo de acesso.
public interface IAccessAuditService
{
    /// <summary>
    /// Registra um evento operacional de autenticacao ou sessao.
    /// </summary>
    Task RegistrarEventoAcessoAsync(Guid usuarioId, string tipoEvento, object? detalhes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra uma trilha de auditoria de alteracao de entidade.
    /// </summary>
    Task RegistrarAuditoriaAsync(
        Guid? lojaId,
        string entidade,
        Guid entidadeId,
        string acao,
        object? antes,
        object? depois,
        CancellationToken cancellationToken = default);
}
