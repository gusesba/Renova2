namespace Renova.Domain.Model.Dto
{
    public class AtualizacaoPendenciaClienteDto
    {
        public int ClienteId { get; set; }
        public required string Nome { get; set; }
        public int QuantidadeOrdensAtualizadas { get; set; }
        public decimal ValorAtualizado { get; set; }
    }
}
