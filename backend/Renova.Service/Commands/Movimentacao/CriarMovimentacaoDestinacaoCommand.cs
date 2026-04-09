using System.ComponentModel.DataAnnotations;

using Renova.Domain.Model;

namespace Renova.Service.Commands.Movimentacao
{
    public class CriarMovimentacaoDestinacaoCommand
    {
        [Required]
        public DateTime Data { get; set; }

        [Required]
        public int LojaId { get; set; }

        [Required]
        [MinLength(1)]
        public required List<CriarMovimentacaoDestinacaoItemCommand> Itens { get; set; }
    }

    public class CriarMovimentacaoDestinacaoItemCommand
    {
        [Required]
        public int ProdutoId { get; set; }

        [Required]
        [EnumDataType(typeof(TipoMovimentacao))]
        public TipoMovimentacao Tipo { get; set; }
    }
}
