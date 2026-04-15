using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Queries.Cliente
{
    public class ExportarFechamentoClientesQuery
    {
        [Required]
        public int? LojaId { get; set; }

        [Required]
        public DateTime? DataInicial { get; set; }

        [Required]
        public DateTime? DataFinal { get; set; }
    }
}
