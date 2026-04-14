namespace Renova.Domain.Model
{
    public class LojaModel
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public int UsuarioId { get; set; }
        public UsuarioModel? Usuario { get; set; }
        public ConfigLojaModel? ConfigLoja { get; set; }
        public ICollection<ClienteModel> Clientes { get; set; } = [];
        public ICollection<ClienteCreditoModel> CreditosClientes { get; set; } = [];
        public ICollection<ProdutoEstoqueModel> ProdutosEstoque { get; set; } = [];
        public ICollection<SolicitacaoModel> Solicitacoes { get; set; } = [];
        public ICollection<MovimentacaoModel> Movimentacoes { get; set; } = [];
        public ICollection<GastoLojaModel> GastosLoja { get; set; } = [];
        public ICollection<PagamentoModel> Pagamentos { get; set; } = [];
        public ICollection<PagamentoCreditoModel> PagamentosCredito { get; set; } = [];
        public ICollection<ProdutoReferenciaModel> ProdutosReferencia { get; set; } = [];
        public ICollection<MarcaModel> Marcas { get; set; } = [];
        public ICollection<TamanhoModel> Tamanhos { get; set; } = [];
        public ICollection<CorModel> Cores { get; set; } = [];
    }
}
