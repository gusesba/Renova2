using System.ComponentModel.DataAnnotations;

using Renova.Domain.Model;
using Renova.Service.Queries.Common;

namespace Renova.Service.Queries.Pagamento
{
    public class ObterPagamentosQuery : PaginacaoQuery
    {
        [Required]
        public int? LojaId { get; set; }

        public DateTime? DataInicial { get; set; }

        public DateTime? DataFinal { get; set; }

        public string? Cliente { get; set; }

        public int? MovimentacaoId { get; set; }

        public NaturezaPagamento? Natureza { get; set; }

        public StatusPagamento? Status { get; set; }
    }
}
