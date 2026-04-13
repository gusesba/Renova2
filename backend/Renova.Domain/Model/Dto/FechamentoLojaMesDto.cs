namespace Renova.Domain.Model.Dto
{
    public class FechamentoLojaMesDto
    {
        public int Ano { get; set; }

        public int Mes { get; set; }

        public DateTime InicioPeriodo { get; set; }

        public int QuantidadePecasVendidas { get; set; }

        public decimal ValorRecebidoClientes { get; set; }

        public decimal ValorPagoFornecedores { get; set; }

        public decimal Total { get; set; }
    }
}
