namespace Renova.Domain.Models;

public class AuditoriaEvento : AuditEntityBase
{
    public Guid? LojaId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Entidade { get; set; } = string.Empty;
    public Guid EntidadeId { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? AntesJson { get; set; }
    public string? DepoisJson { get; set; }
    public DateTimeOffset OcorridoEm { get; set; } = DateTimeOffset.UtcNow;
}
