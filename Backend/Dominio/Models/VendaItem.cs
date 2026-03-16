namespace Renova.Domain.Models;

public class VendaItem : AuditEntityBase
{
    public Guid VendaId { get; set; }
    public Guid PecaId { get; set; }
    public int Quantidade { get; set; } = 1;
    public decimal PrecoTabelaUnitario { get; set; }
    public decimal DescontoUnitario { get; set; }
    public decimal PrecoFinalUnitario { get; set; }
    public string TipoPecaSnapshot { get; set; } = string.Empty;
    public Guid? FornecedorPessoaIdSnapshot { get; set; }
    public decimal? PercentualRepasseDinheiroSnapshot { get; set; }
    public decimal? PercentualRepasseCreditoSnapshot { get; set; }
    public decimal ValorRepassePrevisto { get; set; }
}
