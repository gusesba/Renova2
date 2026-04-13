namespace Renova.Domain.Model.Dto
{
    public class PagamentoCreditoDto
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public int ClienteId { get; set; }
        public TipoPagamentoCredito Tipo { get; set; }
        public int? ConfigLojaFormaPagamentoId { get; set; }
        public string? FormaPagamentoNome { get; set; }
        public decimal ValorCredito { get; set; }
        public decimal ValorDinheiro { get; set; }
        public DateTime Data { get; set; }
    }
}
