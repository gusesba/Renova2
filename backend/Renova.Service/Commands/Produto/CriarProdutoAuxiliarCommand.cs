using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Produto
{
    public class CriarProdutoAuxiliarCommand
    {
        [Required]
        [MinLength(1)]
        public required string Valor { get; set; }

        [Required]
        public int LojaId { get; set; }
    }
}
