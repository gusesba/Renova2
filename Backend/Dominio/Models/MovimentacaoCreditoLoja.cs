namespace Renova.Domain.Models;

public class MovimentacaoCreditoLoja : AuditEntityBase
{
    public Guid ContaCreditoLojaId { get; set; }
    public string TipoMovimentacao { get; set; } = string.Empty;
    public string OrigemTipo { get; set; } = string.Empty;
    public Guid? OrigemId { get; set; }
    public decimal Valor { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoPosterior { get; set; }
    public string Observacoes { get; set; } = string.Empty;
    public DateTimeOffset MovimentadoEm { get; set; } = DateTimeOffset.UtcNow;
    public Guid MovimentadoPorUsuarioId { get; set; }
}
