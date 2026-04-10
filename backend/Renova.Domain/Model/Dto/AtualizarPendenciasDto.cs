namespace Renova.Domain.Model.Dto
{
    public class AtualizarPendenciasDto
    {
        public int QuantidadeOrdensAtualizadas { get; set; }
        public decimal ValorTotalCredito { get; set; }
        public IReadOnlyList<AtualizacaoPendenciaClienteDto> ClientesAtualizados { get; set; } = [];
    }
}
