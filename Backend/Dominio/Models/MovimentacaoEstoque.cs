namespace Renova.Domain.Models;

public class MovimentacaoEstoque : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public Guid PecaId { get; set; }
    public string TipoMovimentacao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int SaldoAnterior { get; set; }
    public int SaldoPosterior { get; set; }
    public string OrigemTipo { get; set; } = string.Empty;
    public Guid? OrigemId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTimeOffset MovimentadoEm { get; set; } = DateTimeOffset.UtcNow;
    public Guid MovimentadoPorUsuarioId { get; set; }
}
