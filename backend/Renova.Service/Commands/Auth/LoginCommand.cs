using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Auth
{
    public class LoginCommand
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Senha { get; set; }
    }
}