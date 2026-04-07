namespace Renova.Domain.Model
{
    public class ProdutoReferenciaModel
    {
        public int Id { get; set; }
        public required string Valor { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public ICollection<ProdutoEstoqueModel> ProdutosEstoque { get; set; } = [];
    }
}
