namespace Renova.Domain.Models;

public class Marca : AtivavelEntityBase
{
    public Guid ConjuntoCatalogoId { get; set; }
    public string Nome { get; set; } = string.Empty;
}
