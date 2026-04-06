namespace Renova.Domain.Model.Dto
{
    public class PaginacaoDto<T>
    {
        public required IReadOnlyList<T> Itens { get; set; }
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalItens { get; set; }
        public int TotalPaginas { get; set; }
    }
}
