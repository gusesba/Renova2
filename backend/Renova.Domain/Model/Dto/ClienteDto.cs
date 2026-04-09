namespace Renova.Domain.Model.Dto
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Contato { get; set; }
        public bool Doacao { get; set; }
        public int LojaId { get; set; }
        public int? UserId { get; set; }
    }
}
