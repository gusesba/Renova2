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

        [Required]
        [MinLength(1)]
        public required List<int> ProdutoIds { get; set; }
    }
}
