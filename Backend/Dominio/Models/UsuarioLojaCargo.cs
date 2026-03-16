namespace Renova.Domain.Models;

public class UsuarioLojaCargo : AuditEntityBase
{
    public Guid UsuarioLojaId { get; set; }
    public Guid CargoId { get; set; }
}
