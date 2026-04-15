namespace Renova.Domain.Model.Dto
{
    public class FuncionarioDto
    {
        public int UsuarioId { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public int LojaId { get; set; }
    }
}
