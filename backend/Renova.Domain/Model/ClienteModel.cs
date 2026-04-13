namespace Renova.Domain.Model
{
    public class ClienteModel
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Contato { get; set; }
        public bool Doacao { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public int? UserId { get; set; }
        public UsuarioModel? User { get; set; }
        public ClienteCreditoModel? Credito { get; set; }
        public ICollection<ProdutoEstoqueModel> ProdutosFornecidos { get; set; } = [];
        public ICollection<SolicitacaoModel> Solicitacoes { get; set; } = [];
        public ICollection<MovimentacaoModel> Movimentacoes { get; set; } = [];
        public ICollection<PagamentoModel> Pagamentos { get; set; } = [];
        public ICollection<PagamentoCreditoModel> PagamentosCredito { get; set; } = [];
    }
}
