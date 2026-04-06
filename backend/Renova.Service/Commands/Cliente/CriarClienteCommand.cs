using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Cliente
{
    public class CriarClienteCommand
    {
        [Required]
        [MinLength(1)]
        public required string Nome { get; set; }

        [Required]
        [MinLength(1)]
        public required string Contato { get; set; }

        [Required]
        public int LojaId { get; set; }

        public int? UserId { get; set; }
    }
}
