namespace Renova.Domain.Models;

public class Categoria : AtivavelEntityBase
{
    public Guid ConjuntoCatalogoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
