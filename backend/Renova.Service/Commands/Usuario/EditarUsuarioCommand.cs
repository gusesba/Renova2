using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Usuario
{
    public class EditarUsuarioCommand
    {
        [Required]
        [MinLength(1)]
        public required string Nome { get; set; }
    }
}
