namespace Renova.Domain.Model.Dto
{
    public class SolicitacaoDto
    {
        public int Id { get; set; }
        public int? ProdutoId { get; set; }
        public int? MarcaId { get; set; }
        public int? TamanhoId { get; set; }
        public int? CorId { get; set; }
        public int? ClienteId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal? PrecoMinimo { get; set; }
        public decimal? PrecoMaximo { get; set; }
        public int LojaId { get; set; }
        public IReadOnlyList<ProdutoCompativelDto> ProdutosCompativeis { get; set; } = [];
    }
}
