namespace Renova.Domain.Model.Dto
{
    public class ProdutoCompativelDto
    {
        public int Id { get; set; }
        public required string Produto { get; set; }
        public required string Marca { get; set; }
        public required string Tamanho { get; set; }
        public required string Cor { get; set; }
        public required string Fornecedor { get; set; }
        public required string Descricao { get; set; }
        public decimal Preco { get; set; }
    }
}
