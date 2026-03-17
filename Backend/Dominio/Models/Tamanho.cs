namespace Renova.Domain.Models;

public class Tamanho : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public string Nome { get; set; } = string.Empty;
}
