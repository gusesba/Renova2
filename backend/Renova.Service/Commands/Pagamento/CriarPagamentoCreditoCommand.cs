using Renova.Domain.Model;

namespace Renova.Service.Commands.Pagamento
{
    public class CriarPagamentoCreditoCommand
    {
        public int LojaId { get; set; }
        public int ClienteId { get; set; }
        public TipoPagamentoCredito Tipo { get; set; }
        public int? ConfigLojaFormaPagamentoId { get; set; }
        public decimal ValorCredito { get; set; }
        public DateTime Data { get; set; }
    }
}
