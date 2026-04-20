using Renova.Service.Queries.Common;

namespace Renova.Service.Queries.Cliente
{
    public class ObterMinhasPecasQuery : PaginacaoQuery
    {
        public string? Loja { get; set; }
        public string? Produto { get; set; }
        public string? Descricao { get; set; }
        public string? Marca { get; set; }
        public string? Tamanho { get; set; }
        public string? Cor { get; set; }
        public decimal? PrecoInicial { get; set; }
        public decimal? PrecoFinal { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
    }
}
