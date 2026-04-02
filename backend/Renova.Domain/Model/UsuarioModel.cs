namespace Renova.Domain.Model
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string SenhaHash { get; set; }
        public ICollection<LojaModel> Lojas { get; set; } = [];
    }
}