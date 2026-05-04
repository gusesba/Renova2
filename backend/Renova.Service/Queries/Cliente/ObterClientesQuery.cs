using System.ComponentModel.DataAnnotations;

using Renova.Service.Queries.Common;

namespace Renova.Service.Queries.Cliente
{
    public class ObterClientesQuery : PaginacaoQuery
    {
        [Required]
        public int? LojaId { get; set; }

        public int? Id { get; set; }

        public string? Nome { get; set; }

        public string? Contato { get; set; }
    }
}
