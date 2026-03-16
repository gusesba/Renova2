namespace Renova.Domain.Models;

public class ContaCreditoLoja : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public Guid PessoaId { get; set; }
    public decimal SaldoAtual { get; set; }
    public decimal SaldoComprometido { get; set; }
    public string StatusConta { get; set; } = string.Empty;
}
