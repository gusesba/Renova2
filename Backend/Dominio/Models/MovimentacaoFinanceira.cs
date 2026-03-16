namespace Renova.Domain.Models;

public class MovimentacaoFinanceira : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public string TipoMovimentacao { get; set; } = string.Empty;
    public string Direcao { get; set; } = string.Empty;
    public Guid? MeioPagamentoId { get; set; }
    public Guid? VendaPagamentoId { get; set; }
    public Guid? LiquidacaoObrigacaoFornecedorId { get; set; }
    public decimal ValorBruto { get; set; }
    public decimal Taxa { get; set; }
    public decimal ValorLiquido { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTimeOffset? CompetenciaEm { get; set; }
    public DateTimeOffset MovimentadoEm { get; set; } = DateTimeOffset.UtcNow;
    public Guid MovimentadoPorUsuarioId { get; set; }
}
