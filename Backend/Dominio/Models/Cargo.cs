namespace Renova.Domain.Models;

public class Cargo : AtivavelEntityBase
{
    public Guid LojaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
