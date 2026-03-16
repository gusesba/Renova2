namespace Renova.Domain.Models;

public class Usuario : AuditEntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string SenhaSalt { get; set; } = string.Empty;
    public string StatusUsuario { get; set; } = string.Empty;
    public DateTimeOffset? UltimoLoginEm { get; set; }
    public Guid? PessoaId { get; set; }
}
