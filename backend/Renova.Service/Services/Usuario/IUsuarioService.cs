using Renova.Domain.Model.Dto;
using Renova.Service.Queries.Usuario;

namespace Renova.Service.Services.Usuario
{
    public interface IUsuarioService
    {
        Task<PaginacaoDto<UsuarioDto>> GetAllAsync(ObterUsuariosQuery request, int usuarioAutenticadoId, CancellationToken cancellationToken = default);
    }
}
