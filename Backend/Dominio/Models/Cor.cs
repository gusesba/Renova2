namespace Renova.Domain.Models;

public class Cor : AtivavelEntityBase
{
    public Guid ConjuntoCatalogoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Hexadecimal { get; set; }
}
