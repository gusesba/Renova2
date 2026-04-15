namespace Renova.Domain.Model
{
    public class FuncionarioModel
    {
        public int UsuarioId { get; set; }
        public UsuarioModel? Usuario { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
    }
}
