namespace Renova.Domain.Model
{
    public class ConfigLojaModel
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public decimal PercentualRepasseFornecedor { get; set; }
        public decimal PercentualRepasseVendedorCredito { get; set; }
        public int TempoPermanenciaProdutoMeses { get; set; }
        public List<ConfigLojaDescontoPermanenciaModel> DescontosPermanencia { get; set; } = [];
    }
}
