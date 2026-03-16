using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Renova.Api.Infrastructure.Security;

// Representa o provider que cria policies dinamicas a partir de codigos de permissao.
public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public const string PolicyPrefix = "permission:";

    /// <summary>
    /// Inicializa o provider com as opcoes de autorizacao do ASP.NET Core.
    /// </summary>
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    /// <summary>
    /// Resolve ou monta uma policy de permissao em tempo de execucao.
    /// </summary>
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionCode = policyName[PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder(SessionTokenAuthenticationDefaults.SchemeName)
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permissionCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return base.GetPolicyAsync(policyName);
    }

    /// <summary>
    /// Converte um codigo de permissao no nome interno da policy.
    /// </summary>
    public static string BuildPolicyName(string permissionCode) => $"{PolicyPrefix}{permissionCode}";
}
