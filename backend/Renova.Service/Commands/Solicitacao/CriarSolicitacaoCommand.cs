using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Solicitacao
{
    public class CriarSolicitacaoCommand
    {
        public int? ProdutoId { get; set; }

        public int? MarcaId { get; set; }

        public int? TamanhoId { get; set; }

        public int? CorId { get; set; }

        [Required(ErrorMessage = "Cliente e obrigatorio.")]
        public int? ClienteId { get; set; }

        public string? Descricao { get; set; }

        public decimal? PrecoMinimo { get; set; }
        public decimal? PrecoMaximo { get; set; }

        [Required]
        public int LojaId { get; set; }
    }
}
