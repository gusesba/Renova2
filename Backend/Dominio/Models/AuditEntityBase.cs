namespace Renova.Domain.Models;

public abstract class AuditEntityBase : EntityBase
{
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public Guid? CriadoPorUsuarioId { get; set; }
    public DateTimeOffset? AtualizadoEm { get; set; }
    public Guid? AtualizadoPorUsuarioId { get; set; }
    public byte[]? RowVersion { get; set; }
}
