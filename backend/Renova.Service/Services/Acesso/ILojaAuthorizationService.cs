using Renova.Domain.Model.Dto;

namespace Renova.Service.Services.Acesso
{
    public interface ILojaAuthorizationService
    {
        Task EnsureStoreAccessAsync(int lojaId, int usuarioId, CancellationToken cancellationToken = default);
        Task EnsurePermissionAsync(int lojaId, int usuarioId, string funcionalidadeChave, CancellationToken cancellationToken = default);
        Task<AcessoLojaDto> GetAccessAsync(int lojaId, int usuarioId, CancellationToken cancellationToken = default);
    }
}
