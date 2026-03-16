namespace Renova.Domain.Models;

public class Colecao : AtivavelEntityBase
{
    public Guid ConjuntoCatalogoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? AnoReferencia { get; set; }
}
