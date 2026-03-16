namespace Renova.Domain.Models;

public class CargoPermissao : AuditEntityBase
{
    public Guid CargoId { get; set; }
    public Guid PermissaoId { get; set; }
}
