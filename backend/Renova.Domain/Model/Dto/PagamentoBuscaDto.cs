namespace Renova.Domain.Model.Dto
{
    public class PagamentoBuscaDto
    {
        public int Id { get; set; }
        public int? MovimentacaoId { get; set; }
        public int LojaId { get; set; }
        public int ClienteId { get; set; }
        public required string Cliente { get; set; }
        public NaturezaPagamento Natureza { get; set; }
        public StatusPagamento Status { get; set; }
        public string? Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public MovimentacaoResumoDto? Movimentacao { get; set; }
    }
}
