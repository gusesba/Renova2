namespace Renova.Domain.Models;

public class PessoaContaBancaria : AuditEntityBase
{
    public Guid PessoaId { get; set; }
    public string Banco { get; set; } = string.Empty;
    public string Agencia { get; set; } = string.Empty;
    public string Conta { get; set; } = string.Empty;
    public string TipoConta { get; set; } = string.Empty;
    public string PixTipo { get; set; } = string.Empty;
    public string PixChave { get; set; } = string.Empty;
    public string FavorecidoNome { get; set; } = string.Empty;
    public string FavorecidoDocumento { get; set; } = string.Empty;
    public bool Principal { get; set; }
}
