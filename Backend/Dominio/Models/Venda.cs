namespace Renova.Domain.Models;

public class Venda : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public string NumeroVenda { get; set; } = string.Empty;
    public string StatusVenda { get; set; } = string.Empty;
    public DateTimeOffset DataHoraVenda { get; set; } = DateTimeOffset.UtcNow;
    public Guid VendedorUsuarioId { get; set; }
    public Guid? CompradorPessoaId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescontoTotal { get; set; }
    public decimal TaxaTotal { get; set; }
    public decimal TotalLiquido { get; set; }
    public string Observacoes { get; set; } = string.Empty;
    public DateTimeOffset? CanceladaEm { get; set; }
    public Guid? CanceladaPorUsuarioId { get; set; }
    public string? MotivoCancelamento { get; set; }
}
