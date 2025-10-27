using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Hickory.Api.Infrastructure.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Status = response.StatusCode;
                errorResponse.Title = "Validation Error";
                errorResponse.Errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());
                
                _logger.LogWarning(
                    exception,
                    "Validation error: {ValidationErrors}",
                    string.Join("; ", validationException.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Status = response.StatusCode;
                errorResponse.Title = "Unauthorized";
                errorResponse.Detail = exception.Message;
                
                _logger.LogWarning(exception, "Unauthorized access attempt");
                break;

            case InvalidOperationException invalidOperationException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.Status = response.StatusCode;
                errorResponse.Title = "Operation Failed";
                errorResponse.Detail = invalidOperationException.Message;
                
                _logger.LogWarning(exception, "Invalid operation: {Message}", exception.Message);
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Status = response.StatusCode;
                errorResponse.Title = "Resource Not Found";
                errorResponse.Detail = exception.Message;
                
                _logger.LogWarning(exception, "Resource not found");
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Status = response.StatusCode;
                errorResponse.Title = "Internal Server Error";
                errorResponse.Detail = "An unexpected error occurred";
                
                _logger.LogError(exception, "Unhandled exception occurred");
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, JsonOptions);
        await response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public int Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
