using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Loja
{
    public class CriarLojaCommand
    {
        [Required]
        [MinLength(1)]
        public required string Nome { get; set; }
    }
}