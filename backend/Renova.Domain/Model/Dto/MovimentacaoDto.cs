namespace Renova.Domain.Model.Dto
{
    public class MovimentacaoDto
    {
        public int Id { get; set; }
        public TipoMovimentacao Tipo { get; set; }
        public DateTime Data { get; set; }
        public int ClienteId { get; set; }
        public int LojaId { get; set; }
        public decimal? CreditoPendenteCliente { get; set; }
        public required IReadOnlyCollection<int> ProdutoIds { get; set; }
    }
}
