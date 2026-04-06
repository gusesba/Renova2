using System.ComponentModel.DataAnnotations;

namespace Renova.Service.Queries.Common
{
    public abstract class PaginacaoQuery
    {
        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int TamanhoPagina { get; set; } = 10;

        public string? OrdenarPor { get; set; }

        public string Direcao { get; set; } = "asc";
    }
}
