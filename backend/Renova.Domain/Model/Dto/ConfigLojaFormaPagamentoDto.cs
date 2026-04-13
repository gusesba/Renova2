namespace Renova.Domain.Model.Dto
{
    public class ConfigLojaFormaPagamentoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal PercentualAjuste { get; set; }
    }
}
