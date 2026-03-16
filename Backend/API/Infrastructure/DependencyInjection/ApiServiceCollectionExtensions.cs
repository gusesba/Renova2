using System.IO;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Renova.Api.Infrastructure.ExceptionHandling;
using Renova.Api.Infrastructure.Security;
using Renova.Persistence;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services;

namespace Renova.Api.Infrastructure.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddRenovaApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var dataProtectionPath = Path.Combine(environment.ContentRootPath, "App_Data", "Keys");
        Directory.CreateDirectory(dataProtectionPath);

        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<ICurrentRequestContext, HttpCurrentRequestContext>();

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));

        services.AddAuthentication(SessionTokenAuthenticationDefaults.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, SessionTokenAuthenticationHandler>(
                SessionTokenAuthenticationDefaults.SchemeName,
                _ => { });

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

        services.AddCors(options =>
        {
            options.AddPolicy("renova-dev", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddAuthorization();
        services.AddControllers();
        services.AddOpenApi();
        services.AddRenovaApplication();
        services.AddRenovaPersistence(configuration, environment.IsDevelopment());

        return services;
    }
}
