namespace Renova.Domain.Model.Dto
{
    public class ClienteDetalheDto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Contato { get; set; }
        public string? Obs { get; set; }
        public bool Doacao { get; set; }
        public int LojaId { get; set; }
        public int? UserId { get; set; }
        public string? UserNome { get; set; }
        public string? UserEmail { get; set; }
        public int QuantidadePecasCompradas { get; set; }
        public int QuantidadePecasVendidas { get; set; }
        public decimal ValorRetiradoLoja { get; set; }
        public decimal ValorAportadoLoja { get; set; }
        public IReadOnlyList<ProdutoBuscaDto> ProdutosFornecedor { get; set; } = [];
        public IReadOnlyList<ProdutoBuscaDto> ProdutosComCliente { get; set; } = [];
    }
}
