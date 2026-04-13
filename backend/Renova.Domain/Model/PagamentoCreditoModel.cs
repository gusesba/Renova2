namespace Renova.Domain.Model
{
    public class PagamentoCreditoModel
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public int ClienteId { get; set; }
        public ClienteModel? Cliente { get; set; }
        public TipoPagamentoCredito Tipo { get; set; }
        public int? ConfigLojaFormaPagamentoId { get; set; }
        public ConfigLojaFormaPagamentoModel? ConfigLojaFormaPagamento { get; set; }
        public decimal ValorCredito { get; set; }
        public decimal ValorDinheiro { get; set; }
        public DateTime Data { get; set; }
    }
}
