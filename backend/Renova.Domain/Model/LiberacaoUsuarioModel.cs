namespace Renova.Domain.Model
{
    public class LiberacaoUsuarioModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime LiberadoAte { get; set; }
        public bool Ativo { get; set; } = true;
        public UsuarioModel? Usuario { get; set; }
    }
}
