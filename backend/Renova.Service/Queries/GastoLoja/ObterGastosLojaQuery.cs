using System.ComponentModel.DataAnnotations;

using Renova.Domain.Model;
using Renova.Service.Queries.Common;

namespace Renova.Service.Queries.GastoLoja
{
    public class ObterGastosLojaQuery : PaginacaoQuery
    {
        [Required]
        public int? LojaId { get; set; }

        public DateTime? DataInicial { get; set; }

        public DateTime? DataFinal { get; set; }

        public NaturezaGastoLoja? Natureza { get; set; }

        public string? Descricao { get; set; }
    }
}
