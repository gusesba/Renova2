using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Usuario
{
    public class EditarUsuarioCommand
    {
        [Required]
        [MinLength(1)]
        public required string Nome { get; set; }

        public string? SenhaAtual { get; set; }

        [MinLength(6)]
        public string? NovaSenha { get; set; }
    }
}
