namespace Renova.Domain.Models;

// Persistencia dos filtros frequentes do modulo de relatorios por usuario e loja.
public class RelatorioFiltroSalvo : AtivavelEntityBase
{
    public Guid LojaId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TipoRelatorio { get; set; } = string.Empty;
    public string FiltrosJson { get; set; } = string.Empty;
}
