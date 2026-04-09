using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.ConfigLoja
{
    public class SalvarConfigLojaCommand
    {
        [Required]
        public int LojaId { get; set; }

        [Range(typeof(decimal), "0", "100")]
        public decimal PercentualRepasseFornecedor { get; set; }

        [Range(typeof(decimal), "0", "100")]
        public decimal PercentualRepasseVendedorCredito { get; set; }

        [Range(1, int.MaxValue)]
        public int TempoPermanenciaProdutoMeses { get; set; }
    }
}
