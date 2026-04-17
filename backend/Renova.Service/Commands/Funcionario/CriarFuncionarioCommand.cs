using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Commands.Funcionario
{
    public class CriarFuncionarioCommand
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int UsuarioId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CargoId { get; set; }
    }
}
