using System.ComponentModel.DataAnnotations;

using Renova.Domain.Model;
using Renova.Service.Queries.Common;

namespace Renova.Service.Queries.Movimentacao
{
    public class ObterMovimentacoesQuery : PaginacaoQuery
    {
        [Required]
        public int? LojaId { get; set; }

        public int? Id { get; set; }

        public DateTime? DataInicial { get; set; }

        public DateTime? DataFinal { get; set; }

        public string? Cliente { get; set; }

        public TipoMovimentacao? Tipo { get; set; }
    }
}
