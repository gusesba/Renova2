using Microsoft.AspNetCore.Authorization;

namespace Renova.Api.Infrastructure.Security;

// Representa o requisito de autorizacao baseado em um codigo de permissao.
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Inicializa o requisito com o codigo de permissao a validar.
    /// </summary>
    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }

    public string PermissionCode { get; }
}
