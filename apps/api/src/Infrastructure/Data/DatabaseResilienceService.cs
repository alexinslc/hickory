using System.Data.Common;
using Npgsql;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Hickory.Api.Infrastructure.Data;

/// <summary>
/// Service providing resilience policies for database operations.
/// Implements retry logic and circuit breaker pattern for transient failures.
/// </summary>
public class DatabaseResilienceService
{
    private readonly ResiliencePipeline _pipeline;
    private readonly ILogger<DatabaseResilienceService> _logger;
    private readonly DatabaseOptions _options;
    
    public DatabaseResilienceService(
        ILogger<DatabaseResilienceService> logger,
        DatabaseOptions options)
    {
        _logger = logger;
        _options = options;
        _pipeline = BuildResiliencePipeline();
    }
    
    /// <summary>
    /// Builds a resilience pipeline with retry and circuit breaker strategies.
    /// </summary>
    private ResiliencePipeline BuildResiliencePipeline()
    {
        var pipelineBuilder = new ResiliencePipelineBuilder();
        
        // Add retry policy if enabled
        if (_options.EnableRetryPolicy)
        {
            pipelineBuilder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = _options.RetryCount,
                Delay = TimeSpan.FromSeconds(_options.RetryDelaySeconds),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<DbException>(IsTransientError),
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Database operation failed with transient error. Attempt {Attempt} of {MaxAttempts}. Retrying after {Delay}ms. Error: {Error}",
                        args.AttemptNumber + 1,
                        _options.RetryCount + 1,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    
                    return ValueTask.CompletedTask;
                }
            });
        }
        
        // Add circuit breaker if enabled
        if (_options.EnableCircuitBreaker)
        {
            pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5, // Open circuit if 50% of requests fail
                MinimumThroughput = _options.CircuitBreakerThreshold,
                BreakDuration = TimeSpan.FromSeconds(_options.CircuitBreakerDurationSeconds),
                ShouldHandle = new PredicateBuilder().Handle<DbException>(),
                OnOpened = args =>
                {
                    _logger.LogError(
                        "Database circuit breaker opened after {Threshold} failures. Circuit will remain open for {Duration}s.",
                        _options.CircuitBreakerThreshold,
                        _options.CircuitBreakerDurationSeconds);
                    
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("Database circuit breaker closed. Normal operations resumed.");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    _logger.LogInformation("Database circuit breaker half-open. Testing connection...");
                    return ValueTask.CompletedTask;
                }
            });
        }
        
        return pipelineBuilder.Build();
    }
    
    /// <summary>
    /// Executes an async database operation with resilience policies applied.
    /// </summary>
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, ValueTask<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            async (ctx) => await operation(ctx),
            cancellationToken);
    }
    
    /// <summary>
    /// Executes an async database operation with resilience policies applied.
    /// </summary>
    public async Task ExecuteAsync(
        Func<CancellationToken, ValueTask> operation,
        CancellationToken cancellationToken = default)
    {
        await _pipeline.ExecuteAsync(
            async (ctx) => { await operation(ctx); return ValueTask.CompletedTask; },
            cancellationToken);
    }
    
    /// <summary>
    /// Determines if a database exception is transient and should be retried.
    /// </summary>
    private static bool IsTransientError(DbException exception)
    {
        if (exception is NpgsqlException npgsqlException)
        {
            // PostgreSQL transient error codes
            // See: https://www.postgresql.org/docs/current/errcodes-appendix.html
            return npgsqlException.SqlState switch
            {
                "08000" => true, // connection_exception
                "08003" => true, // connection_does_not_exist
                "08006" => true, // connection_failure
                "40001" => true, // serialization_failure
                "40P01" => true, // deadlock_detected
                "53000" => true, // insufficient_resources
                "53100" => true, // disk_full
                "53200" => true, // out_of_memory
                "53300" => true, // too_many_connections
                "53400" => true, // configuration_limit_exceeded
                "57P03" => true, // cannot_connect_now
                "58000" => true, // system_error
                "58030" => true, // io_error
                _ => false
            };
        }
        
        return false;
    }
}
