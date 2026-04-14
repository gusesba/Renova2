using Renova.Domain.Model;

namespace Renova.Service.Commands.GastoLoja
{
    public class CriarGastoLojaCommand
    {
        public int LojaId { get; set; }
        public NaturezaGastoLoja Natureza { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Descricao { get; set; }
    }
}
