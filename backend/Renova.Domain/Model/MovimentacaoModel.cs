namespace Renova.Domain.Model
{
    public class MovimentacaoModel
    {
        public int Id { get; set; }
        public TipoMovimentacao Tipo { get; set; }
        public DateTime Data { get; set; }
        public int ClienteId { get; set; }
        public ClienteModel? Cliente { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public ICollection<MovimentacaoProdutoModel> Produtos { get; set; } = [];
        public ICollection<PagamentoModel> Pagamentos { get; set; } = [];
    }
}
