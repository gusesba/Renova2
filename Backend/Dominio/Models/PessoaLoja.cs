namespace Renova.Domain.Models;

public class PessoaLoja : AuditEntityBase
{
    public Guid PessoaId { get; set; }
    public Guid LojaId { get; set; }
    public bool EhCliente { get; set; }
    public bool EhFornecedor { get; set; }
    public bool AceitaCreditoLoja { get; set; }
    public string PoliticaPadraoFimConsignacao { get; set; } = string.Empty;
    public string ObservacoesInternas { get; set; } = string.Empty;
    public string StatusRelacao { get; set; } = string.Empty;
}
