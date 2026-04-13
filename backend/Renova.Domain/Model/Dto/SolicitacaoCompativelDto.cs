namespace Renova.Domain.Model.Dto
{
    public class SolicitacaoCompativelDto
    {
        public int Id { get; set; }
        public required string Cliente { get; set; }
        public required string Produto { get; set; }
        public required string Marca { get; set; }
        public required string Tamanho { get; set; }
        public required string Cor { get; set; }
        public required string Descricao { get; set; }
        public decimal PrecoMinimo { get; set; }
        public decimal PrecoMaximo { get; set; }
    }
}
