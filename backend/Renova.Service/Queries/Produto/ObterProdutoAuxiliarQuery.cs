using System.ComponentModel.DataAnnotations;

using Renova.Service.Queries.Common;

namespace Renova.Service.Queries.Produto
{
    public class ObterProdutoAuxiliarQuery : PaginacaoQuery
    {
        [Required]
        public int? LojaId { get; set; }

        public string? Valor { get; set; }
    }
}
