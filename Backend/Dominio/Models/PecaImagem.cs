namespace Renova.Domain.Models;

public class PecaImagem : AuditEntityBase
{
    public Guid PecaId { get; set; }
    public string UrlArquivo { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public string TipoVisibilidade { get; set; } = string.Empty;
}
