namespace Renova.Domain.Models;

public class UsuarioAcessoEvento : AuditEntityBase
{
    public Guid UsuarioId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public DateTimeOffset OcorridoEm { get; set; } = DateTimeOffset.UtcNow;
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? DetalhesJson { get; set; }
}
