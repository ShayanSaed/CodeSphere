using System.Net;
using System.Text.Json;
using CodeSphere.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeSphere.Api.Middleware;

/// <summary>
/// Custom middleware (mandatory requirement #7, API side): converts domain
/// exceptions and unhandled exceptions into consistent RFC7807
/// ProblemDetails JSON responses instead of leaking stack traces to API
/// consumers.
/// </summary>
public class ApiExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;

    public ApiExceptionHandlingMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, title) = ex switch
            {
                NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                ForbiddenActionException => (HttpStatusCode.Forbidden, "Action not allowed"),
                BusinessRuleException => (HttpStatusCode.BadRequest, "Business rule violation"),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled API exception on {Path}", context.Request.Path);
            else
                _logger.LogWarning(ex, "{Title} on {Path}", title, context.Request.Path);

            var problem = new ProblemDetails
            {
                Title = title,
                Detail = ex.Message,
                Status = (int)statusCode,
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}

public static class ApiExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ApiExceptionHandlingMiddleware>();
}
