namespace Renova.Domain.Models;

public class PecaHistoricoPreco : AuditEntityBase
{
    public Guid PecaId { get; set; }
    public decimal PrecoAnterior { get; set; }
    public decimal PrecoNovo { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTimeOffset AlteradoEm { get; set; } = DateTimeOffset.UtcNow;
    public Guid AlteradoPorUsuarioId { get; set; }
}
