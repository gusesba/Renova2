namespace Renova.Domain.Model
{
    public class MovimentacaoProdutoModel
    {
        public int MovimentacaoId { get; set; }
        public MovimentacaoModel? Movimentacao { get; set; }
        public int ProdutoId { get; set; }
        public ProdutoEstoqueModel? Produto { get; set; }
        public decimal Desconto { get; set; }
    }
}
