using Asp.Versioning;
using Renova.Api.Infrastructure.ExceptionHandling;
using Renova.Persistence;
using Renova.Services;

namespace Renova.Api.Infrastructure.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddRenovaApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddAuthorization();
        services.AddControllers();
        services.AddOpenApi();
        services.AddRenovaApplication();
        services.AddRenovaPersistence(configuration, environment.IsDevelopment());

        return services;
    }
}
