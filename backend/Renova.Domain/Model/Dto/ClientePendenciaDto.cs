namespace Renova.Domain.Model.Dto
{
    public class ClientePendenciaDto
    {
        public int ClienteId { get; set; }
        public required string Nome { get; set; }
        public required string Contato { get; set; }
        public decimal Credito { get; set; }
    }
}
