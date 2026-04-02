namespace Renova.Domain.Model.Dto
{
    public class UsuarioTokenDto
    {
        public required UsuarioDto Usuario { get; set; }
        public required string Token { get; set; }
    }
}