using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Usuario;
using Renova.Service.Parameters.Usuario;
using Renova.Service.Queries.Usuario;

namespace Renova.Service.Services.Usuario
{
    public interface IUsuarioService
    {
        Task<PaginacaoDto<UsuarioDto>> GetAllAsync(ObterUsuariosQuery request, int usuarioAutenticadoId, CancellationToken cancellationToken = default);
        Task<UsuarioDto> EditAsync(EditarUsuarioCommand command, EditarUsuarioParametros parametros, CancellationToken cancellationToken = default);
    }
}
