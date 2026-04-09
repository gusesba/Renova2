namespace Renova.Domain.Model
{
    public class PagamentoModel
    {
        public int Id { get; set; }
        public int MovimentacaoId { get; set; }
        public MovimentacaoModel? Movimentacao { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public int ClienteId { get; set; }
        public ClienteModel? Cliente { get; set; }
        public NaturezaPagamento Natureza { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
    }
}
