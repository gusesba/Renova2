namespace Renova.Domain.Model
{
    public class LojaModel
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public int UsuarioId { get; set; }
        public UsuarioModel? Usuario { get; set; }
    }
}
