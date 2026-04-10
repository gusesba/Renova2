using System.ComponentModel.DataAnnotations;

using Renova.Domain.Model;
using Renova.Service.Queries.Common;

namespace Renova.Service.Queries.Pagamento
{
    public class ObterPagamentosCreditoQuery : PaginacaoQuery
    {
        [Required]
        public int? LojaId { get; set; }

        public DateTime? DataInicial { get; set; }

        public DateTime? DataFinal { get; set; }

        public string? Cliente { get; set; }

        public TipoPagamentoCredito? Tipo { get; set; }
    }
}
