using Hickory.Api.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Hickory.Api.Infrastructure.Health;

/// <summary>
/// Health check for database connection pool status.
/// Monitors pool health and warns on potential exhaustion.
/// </summary>
public class DatabasePoolHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly DatabaseOptions _options;
    private readonly ILogger<DatabasePoolHealthCheck> _logger;
    
    public DatabasePoolHealthCheck(
        IConfiguration configuration,
        DatabaseOptions options,
        ILogger<DatabasePoolHealthCheck> logger)
    {
        _configuration = configuration;
        _options = options;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            // Test connection can be opened
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            
            // Check pool configuration
            var data = new Dictionary<string, object>
            {
                ["pooling_enabled"] = _options.EnablePooling,
                ["min_pool_size"] = _options.MinPoolSize,
                ["max_pool_size"] = _options.MaxPoolSize,
                ["connection_lifetime_sec"] = _options.ConnectionLifetimeSeconds,
                ["idle_lifetime_sec"] = _options.ConnectionIdleLifetimeSeconds,
                ["retry_policy_enabled"] = _options.EnableRetryPolicy,
                ["circuit_breaker_enabled"] = _options.EnableCircuitBreaker
            };
            
            // Warn if pool is small relative to expected load
            if (_options.MaxPoolSize < 10)
            {
                _logger.LogWarning(
                    "Database connection pool size ({MaxPoolSize}) may be too small for production load",
                    _options.MaxPoolSize);
                
                return HealthCheckResult.Degraded(
                    "Connection pool size may be insufficient for production",
                    data: data);
            }
            
            return HealthCheckResult.Healthy(
                "Database connection pool is healthy",
                data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database pool health check failed");
            
            return HealthCheckResult.Unhealthy(
                "Failed to connect to database or check pool health",
                exception: ex);
        }
    }
}
