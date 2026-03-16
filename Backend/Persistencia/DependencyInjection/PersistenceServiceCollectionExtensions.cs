using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Renova.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddRenovaPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var connectionString = configuration.GetConnectionString("RenovaDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'RenovaDb' precisa estar configurada para inicializar a aplicação.");
        }

        services.AddDbContext<RenovaDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            if (isDevelopment)
            {
                options.EnableDetailedErrors();
            }
        });

        return services;
    }
}
