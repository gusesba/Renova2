namespace Renova.Domain.Model.Dto
{
    public class ClientePendenciaAreaDto
    {
        public int LojaId { get; set; }

        public required string LojaNome { get; set; }

        public int ClienteId { get; set; }

        public decimal SaldoConta { get; set; }

        public decimal ValorCredito { get; set; }

        public decimal? ValorEspecie { get; set; }

        public required string Situacao { get; set; }
    }
}
