namespace Renova.Domain.Models;

public class AlertaOperacional : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public string TipoAlerta { get; set; } = string.Empty;
    public string Severidade { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ReferenciaTipo { get; set; } = string.Empty;
    public Guid? ReferenciaId { get; set; }
    public string StatusAlerta { get; set; } = string.Empty;
    public DateTimeOffset GeradoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvidoEm { get; set; }
}
