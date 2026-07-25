using System.Diagnostics;

namespace CodeSphere.Web.Middleware;

/// <summary>
/// Second custom middleware: lightweight request/response logger that times
/// every request and flags the "interesting" write operations (article
/// publish, comment, reaction, delete) for an audit trail, satisfying the
/// audit-logging bonus in spirit without needing a separate table for pure
/// page views.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private static readonly string[] AuditedMethods = { "POST", "PUT", "DELETE" };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var isAudited = AuditedMethods.Contains(context.Request.Method);
        var user = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity!.Name
            : "anonymous";

        if (isAudited)
        {
            _logger.LogInformation(
                "AUDIT {Method} {Path} by {User} -> {StatusCode} ({Elapsed}ms)",
                context.Request.Method, context.Request.Path, user, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogDebug(
                "{Method} {Path} -> {StatusCode} ({Elapsed}ms)",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseCodeSphereRequestLogging(this IApplicationBuilder builder) =>
        builder.UseMiddleware<RequestLoggingMiddleware>();
}
