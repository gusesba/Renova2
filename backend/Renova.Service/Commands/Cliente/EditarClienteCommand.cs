using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Cliente
{
    public class EditarClienteCommand
    {
        [Required]
        [MinLength(1)]
        public required string Nome { get; set; }

        [Required]
        [MinLength(1)]
        public required string Contato { get; set; }

        public bool Doacao { get; set; }

        public int? UserId { get; set; }
    }
}
