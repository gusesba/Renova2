using Microsoft.Extensions.DependencyInjection;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Services;
using Renova.Services.Features.Catalogs.Abstractions;
using Renova.Services.Features.Catalogs.Services;
using Renova.Services.Features.CommercialRules.Abstractions;
using Renova.Services.Features.CommercialRules.Services;
using Renova.Services.Features.Consignments.Abstractions;
using Renova.Services.Features.Consignments.Services;
using Renova.Services.Features.Credits.Abstractions;
using Renova.Services.Features.Credits.Services;
using Renova.Services.Features.People.Abstractions;
using Renova.Services.Features.People.Services;
using Renova.Services.Features.Pieces.Abstractions;
using Renova.Services.Features.Pieces.Services;
using Renova.Services.Features.Sales.Abstractions;
using Renova.Services.Features.Sales.Services;
using Renova.Services.Features.StockMovements.Abstractions;
using Renova.Services.Features.StockMovements.Services;
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
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICommercialRuleResolverService, CommercialRuleResolverService>();
        services.AddScoped<ICommercialRuleService, CommercialRuleService>();
        services.AddScoped<IConsignmentService, ConsignmentService>();
        services.AddScoped<ICreditService, CreditService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IPieceService, PieceService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IStockAvailabilityService, StockAvailabilityService>();
        services.AddScoped<IStockMovementService, StockMovementService>();

        return services;
    }
}
