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

        [Range(1, int.MaxValue)]
        public int ProdutosFornecedorPagina { get; set; } = 1;

        [Range(1, 100)]
        public int ProdutosFornecedorTamanhoPagina { get; set; } = 10;

        [Range(1, int.MaxValue)]
        public int ProdutosComClientePagina { get; set; } = 1;

        [Range(1, 100)]
        public int ProdutosComClienteTamanhoPagina { get; set; } = 10;
    }
}
