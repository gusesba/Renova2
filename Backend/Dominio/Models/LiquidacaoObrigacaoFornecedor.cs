namespace Renova.Domain.Models;

public class LiquidacaoObrigacaoFornecedor : AuditEntityBase
{
    public Guid ObrigacaoFornecedorId { get; set; }
    public string TipoLiquidacao { get; set; } = string.Empty;
    public Guid? MeioPagamentoId { get; set; }
    public Guid? ContaCreditoLojaId { get; set; }
    public decimal Valor { get; set; }
    public string? ComprovanteUrl { get; set; }
    public DateTimeOffset LiquidadoEm { get; set; } = DateTimeOffset.UtcNow;
    public Guid LiquidadoPorUsuarioId { get; set; }
    public string Observacoes { get; set; } = string.Empty;
}
