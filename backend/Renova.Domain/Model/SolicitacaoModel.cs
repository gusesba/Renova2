namespace Renova.Domain.Model
{
    public class SolicitacaoModel
    {
        public int Id { get; set; }
        public int? ProdutoId { get; set; }
        public ProdutoReferenciaModel? Produto { get; set; }
        public int? MarcaId { get; set; }
        public MarcaModel? Marca { get; set; }
        public int? TamanhoId { get; set; }
        public TamanhoModel? Tamanho { get; set; }
        public int? CorId { get; set; }
        public CorModel? Cor { get; set; }
        public int? ClienteId { get; set; }
        public ClienteModel? Cliente { get; set; }
        public string? Descricao { get; set; }
        public decimal? PrecoMinimo { get; set; }
        public decimal? PrecoMaximo { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
    }
}
