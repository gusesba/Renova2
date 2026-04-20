namespace Renova.Domain.Model.Dto
{
    public class SolicitacaoBuscaDto
    {
        public int Id { get; set; }
        public int? ProdutoId { get; set; }
        public string Produto { get; set; } = string.Empty;
        public int? MarcaId { get; set; }
        public string Marca { get; set; } = string.Empty;
        public int? TamanhoId { get; set; }
        public string Tamanho { get; set; } = string.Empty;
        public int? CorId { get; set; }
        public string Cor { get; set; } = string.Empty;
        public int? ClienteId { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal? PrecoMinimo { get; set; }
        public decimal? PrecoMaximo { get; set; }
        public int LojaId { get; set; }
        public IReadOnlyList<ProdutoCompativelDto> ProdutosCompativeis { get; set; } = [];
    }
}
