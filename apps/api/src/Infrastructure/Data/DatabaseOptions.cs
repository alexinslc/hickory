namespace Hickory.Api.Infrastructure.Data;

/// <summary>
/// Configuration options for database connection pooling and resilience.
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";
    
    /// <summary>
    /// Minimum number of connections in the pool. Default: 2
    /// </summary>
    public int MinPoolSize { get; set; } = 2;
    
    /// <summary>
    /// Maximum number of connections in the pool. Default: 20
    /// Production recommendation: 10-50 depending on load
    /// </summary>
    public int MaxPoolSize { get; set; } = 20;
    
    /// <summary>
    /// Maximum lifetime of a connection in seconds. Default: 600 (10 minutes)
    /// Connections older than this will be closed and recreated.
    /// </summary>
    public int ConnectionLifetimeSeconds { get; set; } = 600;
    
    /// <summary>
    /// Time in seconds a connection can be idle before being closed. Default: 300 (5 minutes)
    /// </summary>
    public int ConnectionIdleLifetimeSeconds { get; set; } = 300;
    
    /// <summary>
    /// Connection timeout in seconds. Default: 15
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 15;
    
    /// <summary>
    /// Command timeout in seconds. Default: 30
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Enable connection pooling. Default: true
    /// </summary>
    public bool EnablePooling { get; set; } = true;
    
    /// <summary>
    /// Enable retry policy for transient failures. Default: true
    /// </summary>
    public bool EnableRetryPolicy { get; set; } = true;
    
    /// <summary>
    /// Number of retry attempts for transient failures. Default: 3
    /// </summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>
    /// Delay between retries in seconds. Default: 2
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;
    
    /// <summary>
    /// Enable circuit breaker for cascading failure prevention. Default: true
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;
    
    /// <summary>
    /// Number of consecutive failures before opening circuit. Default: 5
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;
    
    /// <summary>
    /// Duration in seconds to keep circuit open. Default: 60
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 60;
}
