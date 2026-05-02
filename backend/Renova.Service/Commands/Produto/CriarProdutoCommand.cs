using System.ComponentModel.DataAnnotations;

using Renova.Domain.Model;

namespace Renova.Service.Commands.Produto
{
    public class CriarProdutoCommand
    {
        public decimal Preco { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Etiqueta deve conter apenas numeros.")]
        public string? Etiqueta { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; } = 1;

        [Required]
        public int ProdutoId { get; set; }

        [Required]
        public int MarcaId { get; set; }

        [Required]
        public int TamanhoId { get; set; }

        [Required]
        public int CorId { get; set; }

        [Required]
        public int FornecedorId { get; set; }

        [Required]
        [MinLength(1)]
        public required string Descricao { get; set; }

        public DateTime Entrada { get; set; }

        [Required]
        public int LojaId { get; set; }

        [Required]
        [EnumDataType(typeof(SituacaoProduto))]
        public SituacaoProduto Situacao { get; set; }

        public bool Consignado { get; set; }
    }
}
