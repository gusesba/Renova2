namespace Renova.Domain.Models;

public class MeioPagamento : AtivavelEntityBase
{
    public Guid LojaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TipoMeioPagamento { get; set; } = string.Empty;
    public decimal TaxaPercentual { get; set; }
    public int PrazoRecebimentoDias { get; set; }
}
