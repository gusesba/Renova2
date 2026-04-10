using System.ComponentModel.DataAnnotations;

using Renova.Domain.Model;

namespace Renova.Service.Commands.Movimentacao
{
    public class CriarMovimentacaoCommand
    {
        [Required]
        [EnumDataType(typeof(TipoMovimentacao))]
        public TipoMovimentacao Tipo { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int LojaId { get; set; }

        [Range(0, 100)]
        public decimal DescontoTotal { get; set; }

        [Required]
        [MinLength(1)]
        public required List<CriarMovimentacaoProdutoCommand> Produtos { get; set; }
    }

    public class CriarMovimentacaoProdutoCommand
    {
        [Required]
        public int ProdutoId { get; set; }

        [Range(0, 100)]
        public decimal Desconto { get; set; }
    }
}
