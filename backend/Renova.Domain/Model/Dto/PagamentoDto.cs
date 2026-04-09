namespace Renova.Domain.Model.Dto
{
    public class PagamentoDto
    {
        public int Id { get; set; }
        public int MovimentacaoId { get; set; }
        public int LojaId { get; set; }
        public int ClienteId { get; set; }
        public NaturezaPagamento Natureza { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
    }
}
