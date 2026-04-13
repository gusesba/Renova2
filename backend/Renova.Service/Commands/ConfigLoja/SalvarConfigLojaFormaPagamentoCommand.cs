using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.ConfigLoja
{
    public class SalvarConfigLojaFormaPagamentoCommand
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Range(typeof(decimal), "-100", "100")]
        public decimal PercentualAjuste { get; set; }
    }
}
