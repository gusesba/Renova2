namespace Renova.Domain.Model
{
    public class ConfigLojaDescontoPermanenciaModel
    {
        public int Id { get; set; }
        public int ConfigLojaId { get; set; }
        public ConfigLojaModel? ConfigLoja { get; set; }
        public int APartirDeMeses { get; set; }
        public decimal PercentualDesconto { get; set; }
    }
}
