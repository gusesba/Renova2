namespace Renova.Domain.Model
{
    public class ConfigLojaFormaPagamentoModel
    {
        public int Id { get; set; }
        public int ConfigLojaId { get; set; }
        public ConfigLojaModel? ConfigLoja { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal PercentualAjuste { get; set; }
    }
}
