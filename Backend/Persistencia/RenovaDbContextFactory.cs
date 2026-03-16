using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Renova.Persistence;

public sealed class RenovaDbContextFactory : IDesignTimeDbContextFactory<RenovaDbContext>
{
    public RenovaDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiPath = ResolveApiPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("RenovaDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'RenovaDb' precisa estar configurada para comandos do EF Core.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<RenovaDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new RenovaDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiPath()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "..", "API"),
            Path.Combine(Directory.GetCurrentDirectory(), "API"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "API"),
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }
        }

        throw new DirectoryNotFoundException("Não foi possível localizar a pasta API para resolver os appsettings.");
    }
}
