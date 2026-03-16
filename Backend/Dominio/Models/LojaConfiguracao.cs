namespace Renova.Domain.Models;

public class LojaConfiguracao : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public string CabecalhoImpressao { get; set; } = string.Empty;
    public string RodapeImpressao { get; set; } = string.Empty;
    public bool UsaModeloUnicoEtiqueta { get; set; } = true;
    public bool UsaModeloUnicoRecibo { get; set; } = true;
    public string FusoHorario { get; set; } = string.Empty;
    public string Moeda { get; set; } = string.Empty;
}
