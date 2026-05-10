using System.Net;
using System.Text.Json;
using StockScreener.Domain.Exceptions;

namespace StockScreener.API.Middleware;

/// <summary>
/// Global exception handler middleware.
/// Maps domain exceptions to appropriate HTTP status codes and returns
/// a consistent JSON error body. Unhandled exceptions return 500.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException           => (HttpStatusCode.NotFound,           "Resource Not Found"),
            DomainValidationException   => (HttpStatusCode.BadRequest,         "Validation Error"),
            ConflictException           => (HttpStatusCode.Conflict,           "Conflict"),
            InvalidOperationException   => (HttpStatusCode.BadRequest,         "Invalid Operation"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,       "Unauthorized"),
            _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        else
            logger.LogWarning(exception, "{Title}: {Message}", title, exception.Message);

        var body = new ErrorResponse(
            Status:  (int)statusCode,
            Title:   title,
            Detail:  exception.Message,
            TraceId: context.TraceIdentifier
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, JsonOptions));
    }
}

/// <summary>RFC 7807-style problem details response body.</summary>
internal record ErrorResponse(int Status, string Title, string Detail, string TraceId);
