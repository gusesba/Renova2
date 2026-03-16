namespace Renova.Domain.Models;

public class FornecedorRegraComercial : AtivavelEntityBase
{
    public Guid PessoaLojaId { get; set; }
    public decimal PercentualRepasseDinheiro { get; set; }
    public decimal PercentualRepasseCredito { get; set; }
    public bool PermitePagamentoMisto { get; set; }
    public int TempoMaximoExposicaoDias { get; set; }
    public string? PoliticaDescontoJson { get; set; }
}
