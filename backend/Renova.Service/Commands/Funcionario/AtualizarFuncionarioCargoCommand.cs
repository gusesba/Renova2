using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Funcionario
{
    public class AtualizarFuncionarioCargoCommand
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int CargoId { get; set; }
    }
}
