namespace Renova.Domain.Models;

public class Tamanho : AtivavelEntityBase
{
    public Guid ConjuntoCatalogoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int OrdemExibicao { get; set; }
}
