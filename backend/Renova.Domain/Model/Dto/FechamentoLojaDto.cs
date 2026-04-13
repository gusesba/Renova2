namespace Renova.Domain.Model.Dto
{
    public class FechamentoLojaDto
    {
        public DateTime DataReferencia { get; set; }

        public DateTime InicioPeriodo { get; set; }

        public DateTime FimPeriodo { get; set; }

        public int QuantidadePecasVendidas { get; set; }

        public decimal ValorRecebidoClientes { get; set; }

        public decimal ValorPagoFornecedores { get; set; }

        public decimal Total { get; set; }

        public IReadOnlyList<FechamentoLojaMesDto> Historico { get; set; } = [];
    }
}
