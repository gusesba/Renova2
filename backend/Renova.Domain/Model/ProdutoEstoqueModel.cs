namespace Renova.Domain.Model
{
    public class ProdutoEstoqueModel
    {
        public int Id { get; set; }
        public decimal Preco { get; set; }
        public int ProdutoId { get; set; }
        public ProdutoReferenciaModel? Produto { get; set; }
        public int MarcaId { get; set; }
        public MarcaModel? Marca { get; set; }
        public int TamanhoId { get; set; }
        public TamanhoModel? Tamanho { get; set; }
        public int CorId { get; set; }
        public CorModel? Cor { get; set; }
        public int FornecedorId { get; set; }
        public ClienteModel? Fornecedor { get; set; }
        public required string Descricao { get; set; }
        public DateTime Entrada { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public SituacaoProduto Situacao { get; set; }
        public bool Consignado { get; set; }
    }
}
