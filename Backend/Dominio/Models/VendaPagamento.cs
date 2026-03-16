namespace Renova.Domain.Models;

public class VendaPagamento : AuditEntityBase
{
    public Guid VendaId { get; set; }
    public int Sequencia { get; set; }
    public Guid? MeioPagamentoId { get; set; }
    public string TipoPagamento { get; set; } = string.Empty;
    public Guid? ContaCreditoLojaId { get; set; }
    public decimal Valor { get; set; }
    public decimal TaxaPercentualAplicada { get; set; }
    public decimal ValorLiquido { get; set; }
    public DateTimeOffset RecebidoEm { get; set; } = DateTimeOffset.UtcNow;
}
