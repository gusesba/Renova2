namespace Renova.Domain.Model.Dto
{
    public class MovimentacaoResumoDto
    {
        public int Id { get; set; }
        public TipoMovimentacao Tipo { get; set; }
        public DateTime Data { get; set; }
        public int ClienteId { get; set; }
        public required string Cliente { get; set; }
        public int LojaId { get; set; }
        public int QuantidadeProdutos { get; set; }
        public required IReadOnlyCollection<int> ProdutoIds { get; set; }
    }
}
