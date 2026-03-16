using Microsoft.Extensions.DependencyInjection;

namespace Renova.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRenovaApplication(this IServiceCollection services)
    {
        return services;
    }
}
