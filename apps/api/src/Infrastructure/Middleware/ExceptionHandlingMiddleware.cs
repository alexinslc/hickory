using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Hickory.Api.Infrastructure.Middleware;

/// <summary>
/// Global exception handling middleware that returns RFC 7807 Problem Details responses.
/// The correlation ID is automatically included in logs via Serilog's LogContext
/// (enriched by CorrelationIdMiddleware, which runs before this middleware).
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
        response.ContentType = "application/problem+json";

        var correlationId = context.Items["CorrelationId"]?.ToString();

        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path
        };
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        if (correlationId is not null)
            problemDetails.Extensions["correlationId"] = correlationId;

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = response.StatusCode;
                problemDetails.Type = "https://httpstatuses.io/400";
                problemDetails.Title = "Validation Error";
                problemDetails.Extensions["errors"] = validationException.Errors
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
                problemDetails.Status = response.StatusCode;
                problemDetails.Type = "https://httpstatuses.io/401";
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = exception.Message;

                _logger.LogWarning(exception, "Unauthorized access attempt");
                break;

            case InvalidOperationException invalidOperationException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                problemDetails.Status = response.StatusCode;
                problemDetails.Type = "https://httpstatuses.io/409";
                problemDetails.Title = "Operation Failed";
                problemDetails.Detail = invalidOperationException.Message;

                _logger.LogWarning(exception, "Invalid operation: {Message}", exception.Message);
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = response.StatusCode;
                problemDetails.Type = "https://httpstatuses.io/404";
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = exception.Message;

                _logger.LogWarning(exception, "Resource not found");
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problemDetails.Status = response.StatusCode;
                problemDetails.Type = "https://httpstatuses.io/500";
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred";

                _logger.LogError(exception, "Unhandled exception occurred");
                break;
        }

        var result = JsonSerializer.Serialize(problemDetails, JsonOptions);
        await response.WriteAsync(result);
    }
}
