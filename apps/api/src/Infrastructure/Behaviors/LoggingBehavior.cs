using System.Diagnostics;
using MediatR;
using Serilog.Context;

namespace Hickory.Api.Infrastructure.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request execution time and details.
/// Enriches log context with the correlation ID from the current HTTP request.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("CorrelationId", correlationId ?? "N/A"))
        {
            _logger.LogInformation(
                "Handling {RequestName} [CorrelationId: {CorrelationId}]",
                requestName,
                correlationId);

            try
            {
                var response = await next();

                stopwatch.Stop();

                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds}ms [CorrelationId: {CorrelationId}]",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "Error handling {RequestName} after {ElapsedMilliseconds}ms [CorrelationId: {CorrelationId}]",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);

                throw;
            }
        }
    }
}
