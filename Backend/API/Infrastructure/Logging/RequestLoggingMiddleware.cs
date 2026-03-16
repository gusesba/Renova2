using System.Diagnostics;

namespace Renova.Api.Infrastructure.Logging;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        context.Response.Headers["X-Correlation-Id"] = context.TraceIdentifier;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                context.TraceIdentifier);
        }
    }
}
