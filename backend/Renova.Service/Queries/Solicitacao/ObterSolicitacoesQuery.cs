using System.ComponentModel.DataAnnotations;

using Renova.Service.Queries.Common;

namespace Renova.Service.Queries.Solicitacao
{
    public class ObterSolicitacoesQuery : PaginacaoQuery
    {
        [Required]
        public int? LojaId { get; set; }

        public int? Id { get; set; }

        public string? Descricao { get; set; }
        public string? Produto { get; set; }
        public string? Marca { get; set; }
        public string? Tamanho { get; set; }
        public string? Cor { get; set; }
        public string? Cliente { get; set; }
        public decimal? PrecoInicial { get; set; }
        public decimal? PrecoFinal { get; set; }
    }
}
