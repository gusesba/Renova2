namespace Renova.Domain.Model.Dto
{
    public class GastoLojaDto
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public NaturezaGastoLoja Natureza { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Descricao { get; set; }
    }
}
