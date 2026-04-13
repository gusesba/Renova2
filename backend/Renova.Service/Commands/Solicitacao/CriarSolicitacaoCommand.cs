using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Solicitacao
{
    public class CriarSolicitacaoCommand
    {
        [Required]
        public int ProdutoId { get; set; }

        [Required]
        public int MarcaId { get; set; }

        [Required]
        public int TamanhoId { get; set; }

        [Required]
        public int CorId { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        [MinLength(1)]
        public required string Descricao { get; set; }

        public decimal PrecoMinimo { get; set; }
        public decimal PrecoMaximo { get; set; }

        [Required]
        public int LojaId { get; set; }
    }
}
