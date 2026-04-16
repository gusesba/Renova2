using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Cargo
{
    public class EditarCargoCommand
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public required string Nome { get; set; }

        [Required]
        public List<int> FuncionalidadeIds { get; set; } = [];
    }
}
