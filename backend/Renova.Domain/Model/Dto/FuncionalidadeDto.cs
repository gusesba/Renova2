namespace Renova.Domain.Model.Dto
{
    public class FuncionalidadeDto
    {
        public int Id { get; set; }
        public required string Chave { get; set; }
        public required string Grupo { get; set; }
        public required string Descricao { get; set; }
    }
}
