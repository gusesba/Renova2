namespace Renova.Domain.Model.Dto
{
    public class ConfigLojaDto
    {
        public int LojaId { get; set; }
        public decimal PercentualRepasseFornecedor { get; set; }
        public decimal PercentualRepasseVendedorCredito { get; set; }
        public int TempoPermanenciaProdutoMeses { get; set; }
    }
}
