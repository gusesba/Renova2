using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Queries.Pagamento
{
    public class ObterFechamentoLojaQuery
    {
        [Required]
        public int? LojaId { get; set; }

        public DateTime? DataReferencia { get; set; }
    }
}
