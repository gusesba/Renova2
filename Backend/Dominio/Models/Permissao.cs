namespace Renova.Domain.Models;

public class Permissao : AtivavelEntityBase
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
}
