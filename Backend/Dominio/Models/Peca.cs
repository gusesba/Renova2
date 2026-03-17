namespace Renova.Domain.Models;

public class Peca : AuditEntityBase
{
    public Guid LojaId { get; set; }
    public Guid? FornecedorPessoaId { get; set; }
    public string TipoPeca { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public Guid ProdutoNomeId { get; set; }
    public Guid MarcaId { get; set; }
    public Guid TamanhoId { get; set; }
    public Guid CorId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
    public DateTimeOffset DataEntrada { get; set; } = DateTimeOffset.UtcNow;
    public int QuantidadeInicial { get; set; } = 1;
    public int QuantidadeAtual { get; set; } = 1;
    public decimal PrecoVendaAtual { get; set; }
    public decimal? CustoUnitario { get; set; }
    public string StatusPeca { get; set; } = string.Empty;
    public string LocalizacaoFisica { get; set; } = string.Empty;
    public Guid ResponsavelCadastroUsuarioId { get; set; }
}
