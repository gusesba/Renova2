namespace Renova.Domain.Models;

// Representa o token de recuperacao de acesso emitido para um usuario.
public class UsuarioRecuperacaoAcesso : AuditEntityBase
{
    public Guid UsuarioId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset SolicitadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiraEm { get; set; }
    public DateTimeOffset? UtilizadoEm { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
