using CodeSphere.Core.Common;

namespace CodeSphere.Web.Middleware;

/// <summary>
/// Custom middleware (mandatory requirement #7): catches unhandled
/// exceptions from anywhere in the Razor Pages pipeline, logs them, and
/// redirects the user to a friendly error page instead of letting a raw 500
/// leak through in production. Distinguishes "expected" domain exceptions
/// (not found, forbidden, business rule) from truly unexpected ones.
///
/// Two things this version deliberately guards against, because both were
/// actively hiding real bugs (e.g. the Register page's missing IEmailSender):
///   1. In Development, unexpected exceptions are rethrown so the
///      Developer Exception Page (registered earlier in the pipeline) can
///      show the real stack trace, instead of always being swallowed into
///      a generic "/Error" redirect.
///   2. If the response has already started (some content was already
///      written before the exception happened), we do NOT attempt to
///      redirect — that itself throws ("headers are read-only, response
///      has already started"), which turns one real error into a second,
///      more confusing one and can look like the page "isn't displaying"
///      at all.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Not found: {Message}", ex.Message);
            RedirectIfPossible(context, $"/Error/NotFound?message={Uri.EscapeDataString(ex.Message)}");
        }
        catch (ForbiddenActionException ex)
        {
            _logger.LogWarning(ex, "Forbidden action: {Message}", ex.Message);
            RedirectIfPossible(context, $"/Error/Forbidden?message={Uri.EscapeDataString(ex.Message)}");
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
            RedirectIfPossible(context, $"/Error/BadRequest?message={Uri.EscapeDataString(ex.Message)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);

            // Don't mask real bugs during development — let the Developer
            // Exception Page (added earlier in the pipeline) show the actual
            // stack trace instead of a generic friendly page.
            if (_env.IsDevelopment())
                throw;

            RedirectIfPossible(context, "/Error");
        }
    }

    private static void RedirectIfPossible(HttpContext context, string location)
    {
        if (context.Response.HasStarted)
            return; // Too late to redirect; avoid throwing a second, more confusing exception.

        context.Response.Redirect(location);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseCodeSphereExceptionHandling(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExceptionHandlingMiddleware>();
}
