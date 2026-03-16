using Renova.Api.Infrastructure.Logging;

namespace Renova.Api.Infrastructure.Hosting;

public static class WebApplicationExtensions
{
    public static WebApplication UseRenovaApi(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseAuthorization();
        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        return app;
    }
}
