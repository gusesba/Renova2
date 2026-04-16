using Renova.Domain.Access;
using Renova.Domain.Model.Dto;

namespace Renova.Service.Services.Acesso
{
    internal sealed class NullLojaAuthorizationService : ILojaAuthorizationService
    {
        public static readonly ILojaAuthorizationService Instance = new NullLojaAuthorizationService();

        private static readonly IReadOnlyList<string> AllPermissions = [.. FuncionalidadeCatalogo.TodasAsChaves];

        private static readonly AcessoLojaDto EmptyAccessTemplate = new()
        {
            LojaId = 0,
            EhDono = true,
            CargoId = null,
            CargoNome = null,
            Funcionalidades = AllPermissions
        };

        private NullLojaAuthorizationService()
        {
        }

        public Task EnsureStoreAccessAsync(int lojaId, int usuarioId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsurePermissionAsync(int lojaId, int usuarioId, string funcionalidadeChave, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<AcessoLojaDto> GetAccessAsync(int lojaId, int usuarioId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AcessoLojaDto
            {
                LojaId = lojaId,
                EhDono = EmptyAccessTemplate.EhDono,
                CargoId = EmptyAccessTemplate.CargoId,
                CargoNome = EmptyAccessTemplate.CargoNome,
                Funcionalidades = AllPermissions
            });
        }
    }
}
