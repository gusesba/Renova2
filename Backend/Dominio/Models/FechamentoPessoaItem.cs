namespace Renova.Domain.Models;

public class FechamentoPessoaItem : AuditEntityBase
{
    public Guid FechamentoPessoaId { get; set; }
    public Guid PecaId { get; set; }
    public string StatusPecaSnapshot { get; set; } = string.Empty;
    public decimal? ValorVendaSnapshot { get; set; }
    public decimal? ValorRepasseSnapshot { get; set; }
    public DateTimeOffset DataEvento { get; set; }
}
