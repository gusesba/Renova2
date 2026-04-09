using Renova.Service.Queries.Common;

namespace Renova.Service.Queries.Usuario
{
    public class ObterUsuariosQuery : PaginacaoQuery
    {
        public string? Busca { get; set; }
    }
}
