namespace Renova.Domain.Models;

public class PecaCondicaoComercial : AuditEntityBase
{
    public Guid PecaId { get; set; }
    public string OrigemRegra { get; set; } = string.Empty;
    public decimal PercentualRepasseDinheiro { get; set; }
    public decimal PercentualRepasseCredito { get; set; }
    public bool PermitePagamentoMisto { get; set; }
    public int TempoMaximoExposicaoDias { get; set; }
    public string? PoliticaDescontoJson { get; set; }
    public DateTimeOffset? DataInicioConsignacao { get; set; }
    public DateTimeOffset? DataFimConsignacao { get; set; }
    public string? DestinoPadraoFimConsignacao { get; set; }
}
