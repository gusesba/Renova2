namespace Renova.Domain.Model.Dto
{
    public class SolicitacaoBuscaDto
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public required string Produto { get; set; }
        public int MarcaId { get; set; }
        public required string Marca { get; set; }
        public int TamanhoId { get; set; }
        public required string Tamanho { get; set; }
        public int CorId { get; set; }
        public required string Cor { get; set; }
        public int ClienteId { get; set; }
        public required string Cliente { get; set; }
        public required string Descricao { get; set; }
        public decimal PrecoMinimo { get; set; }
        public decimal PrecoMaximo { get; set; }
        public int LojaId { get; set; }
        public IReadOnlyList<ProdutoCompativelDto> ProdutosCompativeis { get; set; } = [];
    }
}
