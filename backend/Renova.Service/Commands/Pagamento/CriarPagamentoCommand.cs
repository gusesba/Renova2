using Renova.Domain.Model;

namespace Renova.Service.Commands.Pagamento
{
    public class CriarPagamentoCommand
    {
        public int MovimentacaoId { get; set; }
        public TipoMovimentacao TipoMovimentacao { get; set; }
        public int LojaId { get; set; }
        public int ClienteId { get; set; }
        public required List<int> ProdutoIds { get; set; }
        public DateTime Data { get; set; }
    }
}
