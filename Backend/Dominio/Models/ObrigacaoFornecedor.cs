namespace Renova.Domain.Models;

public class ObrigacaoFornecedor : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public Guid PessoaId { get; set; }
    public Guid? VendaItemId { get; set; }
    public Guid? PecaId { get; set; }
    public string TipoObrigacao { get; set; } = string.Empty;
    public DateTimeOffset DataGeracao { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DataVencimento { get; set; }
    public decimal ValorOriginal { get; set; }
    public decimal ValorEmAberto { get; set; }
    public string StatusObrigacao { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
}
