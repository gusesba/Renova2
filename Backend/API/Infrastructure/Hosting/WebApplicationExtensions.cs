using Renova.Api.Infrastructure.Logging;
using Renova.Services.Features.Access.Abstractions;
using System.Text;

namespace Renova.Api.Infrastructure.Hosting;

public static class WebApplicationExtensions
{
    public static WebApplication UseRenovaApi(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseCors("renova-dev");
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapGet("/swagger/v1/swagger.json", async context =>
            {
                context.Response.Redirect("/openapi/v1.json", permanent: false);
                await Task.CompletedTask;
            })
            .ExcludeFromDescription();

            app.MapGet("/swagger", async context =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(BuildSwaggerUiHtml());
            })
            .ExcludeFromDescription();

            app.MapGet("/swagger/index.html", async context =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(BuildSwaggerUiHtml());
            })
            .ExcludeFromDescription();
        }

        app.MapControllers();

        return app;
    }

    public static async Task InitializeRenovaApiAsync(this IServiceProvider services, bool isDevelopment)
    {
        using var scope = services.CreateScope();
        var bootstrapService = scope.ServiceProvider.GetRequiredService<IAccessBootstrapService>();
        await bootstrapService.InicializarAsync(isDevelopment);
    }

    private static string BuildSwaggerUiHtml()
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"pt-BR\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"utf-8\" />");
        builder.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        builder.AppendLine("  <title>Renova API Swagger</title>");
        builder.AppendLine("  <link rel=\"stylesheet\" href=\"https://unpkg.com/swagger-ui-dist@5/swagger-ui.css\" />");
        builder.AppendLine("  <style>");
        builder.AppendLine("    body { margin: 0; background: #f4f1ea; }");
        builder.AppendLine("    .topbar { display: none; }");
        builder.AppendLine("  </style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("  <div id=\"swagger-ui\"></div>");
        builder.AppendLine("  <script src=\"https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js\"></script>");
        builder.AppendLine("  <script src=\"https://unpkg.com/swagger-ui-dist@5/swagger-ui-standalone-preset.js\"></script>");
        builder.AppendLine("  <script>");
        builder.AppendLine("    window.onload = function () {");
        builder.AppendLine("      window.ui = SwaggerUIBundle({");
        builder.AppendLine("        url: '/openapi/v1.json',");
        builder.AppendLine("        dom_id: '#swagger-ui',");
        builder.AppendLine("        deepLinking: true,");
        builder.AppendLine("        presets: [SwaggerUIBundle.presets.apis, SwaggerUIStandalonePreset],");
        builder.AppendLine("        layout: 'StandaloneLayout'");
        builder.AppendLine("      });");
        builder.AppendLine("    };");
        builder.AppendLine("  </script>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");
        return builder.ToString();
    }
}
