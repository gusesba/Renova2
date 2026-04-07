namespace Renova.Domain.Model.Dto
{
    public class ProdutoAuxiliarDto
    {
        public int Id { get; set; }
        public required string Valor { get; set; }
        public int LojaId { get; set; }
    }
}
