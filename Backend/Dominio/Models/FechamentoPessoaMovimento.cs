namespace Renova.Domain.Models;

public class FechamentoPessoaMovimento : AuditEntityBase
{
    public Guid FechamentoPessoaId { get; set; }
    public string TipoMovimento { get; set; } = string.Empty;
    public string OrigemTipo { get; set; } = string.Empty;
    public Guid? OrigemId { get; set; }
    public DateTimeOffset DataMovimento { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
