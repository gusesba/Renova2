using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.ConfigLoja
{
    public class SalvarConfigLojaDescontoPermanenciaCommand
    {
        [Range(1, int.MaxValue)]
        public int APartirDeMeses { get; set; }

        [Range(typeof(decimal), "0", "100")]
        public decimal PercentualDesconto { get; set; }
    }
}
