using Renova.Domain.Model;

namespace Renova.Service.Commands.Pagamento
{
    public class CriarPagamentoManualCommand
    {
        public int LojaId { get; set; }
        public int ClienteId { get; set; }
        public NaturezaPagamento Natureza { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Descricao { get; set; }
    }
}
