namespace Renova.Domain.Model.Dto
{
    public class MovimentacaoDestinacaoProdutoDto
    {
        public int Id { get; set; }
        public decimal Preco { get; set; }
        public int ProdutoId { get; set; }
        public required string Produto { get; set; }
        public int MarcaId { get; set; }
        public required string Marca { get; set; }
        public int TamanhoId { get; set; }
        public required string Tamanho { get; set; }
        public int CorId { get; set; }
        public required string Cor { get; set; }
        public int FornecedorId { get; set; }
        public required string Fornecedor { get; set; }
        public required string Descricao { get; set; }
        public DateTime Entrada { get; set; }
        public int LojaId { get; set; }
        public SituacaoProduto Situacao { get; set; }
        public bool Consignado { get; set; }
        public TipoMovimentacao TipoSugerido { get; set; }
    }
}
