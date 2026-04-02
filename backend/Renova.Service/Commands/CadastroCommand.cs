using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands;

public class CadastroCommand
{
    [Required]
    [MinLength(1)]
    public required string Nome { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Senha { get; set; }
}
