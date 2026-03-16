namespace Renova.Domain.Models;

public class FechamentoPessoa : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public Guid PessoaId { get; set; }
    public DateTimeOffset PeriodoInicio { get; set; }
    public DateTimeOffset PeriodoFim { get; set; }
    public string StatusFechamento { get; set; } = string.Empty;
    public decimal ValorVendido { get; set; }
    public decimal ValorAReceber { get; set; }
    public decimal ValorPago { get; set; }
    public decimal ValorCompradoNaLoja { get; set; }
    public decimal SaldoFinal { get; set; }
    public string ResumoTexto { get; set; } = string.Empty;
    public string? PdfUrl { get; set; }
    public string? ExcelUrl { get; set; }
    public DateTimeOffset GeradoEm { get; set; } = DateTimeOffset.UtcNow;
    public Guid GeradoPorUsuarioId { get; set; }
    public DateTimeOffset? ConferidoEm { get; set; }
    public Guid? ConferidoPorUsuarioId { get; set; }
}
