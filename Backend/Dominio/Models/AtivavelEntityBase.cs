namespace Renova.Domain.Models;

public abstract class AtivavelEntityBase : AuditEntityBase
{
    public bool Ativo { get; set; } = true;
    public DateTimeOffset? InativadoEm { get; set; }
}
