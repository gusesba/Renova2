namespace Renova.Domain.Model
{
    public class GastoLojaModel
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public NaturezaGastoLoja Natureza { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Descricao { get; set; }
    }
}
