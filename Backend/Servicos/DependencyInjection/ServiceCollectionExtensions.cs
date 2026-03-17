using Microsoft.Extensions.DependencyInjection;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Services;
using Renova.Services.Features.People.Abstractions;
using Renova.Services.Features.People.Services;
using Renova.Services.Features.Stores.Abstractions;
using Renova.Services.Features.Stores.Services;

namespace Renova.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRenovaApplication(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAccessAuditService, AccessAuditService>();
        services.AddScoped<IAccessBootstrapService, AccessBootstrapService>();
        services.AddScoped<IAccessAuthService, AccessAuthService>();
        services.AddScoped<IAccessUserService, AccessUserService>();
        services.AddScoped<IAccessRoleService, AccessRoleService>();
        services.AddScoped<IAccessStoreMembershipService, AccessStoreMembershipService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IPersonService, PersonService>();

        return services;
    }
}
