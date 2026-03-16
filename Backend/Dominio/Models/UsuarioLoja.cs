namespace Renova.Domain.Models;

public class UsuarioLoja : AuditEntityBase
{
    public Guid UsuarioId { get; set; }
    public Guid LojaId { get; set; }
    public string StatusVinculo { get; set; } = string.Empty;
    public bool EhResponsavel { get; set; }
    public DateTimeOffset DataInicio { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DataFim { get; set; }
}
