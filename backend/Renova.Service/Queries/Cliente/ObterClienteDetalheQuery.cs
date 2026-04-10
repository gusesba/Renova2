using System.ComponentModel.DataAnnotations;

using Renova.Domain.Model;

namespace Renova.Service.Queries.Cliente
{
    public class ObterClienteDetalheQuery
    {
        [Required]
        public int? LojaId { get; set; }

        public DateTime? DataInicial { get; set; }

        public DateTime? DataFinal { get; set; }

        public SituacaoProduto? Situacao { get; set; }
    }
}
