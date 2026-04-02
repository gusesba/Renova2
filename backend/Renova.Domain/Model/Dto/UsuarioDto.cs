namespace Renova.Domain.Model.Dto
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
    }
}