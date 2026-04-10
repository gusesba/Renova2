namespace Renova.Domain.Model.Dto
{
    public class PagamentoCreditoBuscaDto
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public int ClienteId { get; set; }
        public required string Cliente { get; set; }
        public TipoPagamentoCredito Tipo { get; set; }
        public decimal ValorCredito { get; set; }
        public decimal ValorDinheiro { get; set; }
        public DateTime Data { get; set; }
    }
}
