using Microsoft.AspNetCore.Authorization;

namespace Renova.Api.Infrastructure.Security;

// Representa o atributo que associa um endpoint a uma policy de permissao.
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Inicializa o atributo montando a policy a partir do codigo informado.
    /// </summary>
    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = PermissionPolicyProvider.BuildPolicyName(permissionCode);
    }
}
