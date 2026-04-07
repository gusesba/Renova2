namespace Renova.Domain.Model
{
    public class LojaModel
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public int UsuarioId { get; set; }
        public UsuarioModel? Usuario { get; set; }
        public ICollection<ClienteModel> Clientes { get; set; } = [];
        public ICollection<ProdutoEstoqueModel> ProdutosEstoque { get; set; } = [];
        public ICollection<ProdutoReferenciaModel> ProdutosReferencia { get; set; } = [];
        public ICollection<MarcaModel> Marcas { get; set; } = [];
        public ICollection<TamanhoModel> Tamanhos { get; set; } = [];
        public ICollection<CorModel> Cores { get; set; } = [];
    }
}
