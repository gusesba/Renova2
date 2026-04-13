namespace Renova.Domain.Model.Dto
{
    public class ProdutoDto
    {
        public int Id { get; set; }
        public decimal Preco { get; set; }
        public int ProdutoId { get; set; }
        public int MarcaId { get; set; }
        public int TamanhoId { get; set; }
        public int CorId { get; set; }
        public int FornecedorId { get; set; }
        public required string Descricao { get; set; }
        public DateTime Entrada { get; set; }
        public int LojaId { get; set; }
        public SituacaoProduto Situacao { get; set; }
        public bool Consignado { get; set; }
        public IReadOnlyList<SolicitacaoCompativelDto> SolicitacoesCompativeis { get; set; } = [];
    }
}
