using System.Diagnostics;
using System.Diagnostics.Metrics;
using Npgsql;

namespace Hickory.Api.Infrastructure.Data;

/// <summary>
/// Provides metrics and monitoring for database connection pooling.
/// Tracks pool usage, connection creation, and performance metrics.
/// </summary>
public class DatabaseMetricsService
{
    private readonly Meter _meter;
    private readonly Counter<long> _connectionOpenedCounter;
    private readonly Counter<long> _connectionClosedCounter;
    private readonly Counter<long> _connectionFailedCounter;
    private readonly Histogram<double> _connectionOpenDuration;
    private readonly ObservableGauge<int> _activeConnectionsGauge;
    private readonly ObservableGauge<int> _idleConnectionsGauge;
    private readonly ObservableGauge<int> _pooledConnectionsGauge;
    
    private readonly ILogger<DatabaseMetricsService> _logger;
    private readonly string _connectionString;
    
    public DatabaseMetricsService(
        ILogger<DatabaseMetricsService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection string not configured");
        
        _meter = new Meter("Hickory.Api.Database", "1.0.0");
        
        // Counters
        _connectionOpenedCounter = _meter.CreateCounter<long>(
            "db.connection.opened",
            description: "Number of database connections opened");
        
        _connectionClosedCounter = _meter.CreateCounter<long>(
            "db.connection.closed",
            description: "Number of database connections closed");
        
        _connectionFailedCounter = _meter.CreateCounter<long>(
            "db.connection.failed",
            description: "Number of failed database connection attempts");
        
        // Histogram
        _connectionOpenDuration = _meter.CreateHistogram<double>(
            "db.connection.open.duration",
            unit: "ms",
            description: "Duration of database connection open operations");
        
        // Gauges for pool statistics
        _activeConnectionsGauge = _meter.CreateObservableGauge(
            "db.pool.connections.active",
            GetActiveConnections,
            description: "Number of active connections in the pool");
        
        _idleConnectionsGauge = _meter.CreateObservableGauge(
            "db.pool.connections.idle",
            GetIdleConnections,
            description: "Number of idle connections in the pool");
        
        _pooledConnectionsGauge = _meter.CreateObservableGauge(
            "db.pool.connections.total",
            GetTotalConnections,
            description: "Total number of connections in the pool");
    }
    
    /// <summary>
    /// Records a successful connection opening with duration.
    /// </summary>
    public void RecordConnectionOpened(double durationMs)
    {
        _connectionOpenedCounter.Add(1);
        _connectionOpenDuration.Record(durationMs);
    }
    
    /// <summary>
    /// Records a connection being closed.
    /// </summary>
    public void RecordConnectionClosed()
    {
        _connectionClosedCounter.Add(1);
    }
    
    /// <summary>
    /// Records a failed connection attempt.
    /// </summary>
    public void RecordConnectionFailed()
    {
        _connectionFailedCounter.Add(1);
    }
    
    /// <summary>
    /// Gets the current number of active connections from the pool.
    /// </summary>
    private int GetActiveConnections()
    {
        try
        {
            var stats = GetPoolStatistics();
            return stats.Active;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve active connection count");
            return 0;
        }
    }
    
    /// <summary>
    /// Gets the current number of idle connections in the pool.
    /// </summary>
    private int GetIdleConnections()
    {
        try
        {
            var stats = GetPoolStatistics();
            return stats.Idle;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve idle connection count");
            return 0;
        }
    }
    
    /// <summary>
    /// Gets the total number of connections in the pool.
    /// </summary>
    private int GetTotalConnections()
    {
        try
        {
            var stats = GetPoolStatistics();
            return stats.Total;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve total connection count");
            return 0;
        }
    }
    
    /// <summary>
    /// Retrieves pool statistics from Npgsql.
    /// </summary>
    private (int Active, int Idle, int Total) GetPoolStatistics()
    {
        // Note: Npgsql doesn't expose pool statistics directly in a simple way.
        // This is a placeholder for custom monitoring. In production, you might:
        // 1. Use Npgsql's built-in performance counters
        // 2. Implement custom connection tracking
        // 3. Use external monitoring tools
        
        // For now, return zeros as placeholders
        // In a real implementation, you'd need to track connections manually
        // or use Npgsql's internal metrics if exposed
        return (0, 0, 0);
    }
    
    /// <summary>
    /// Logs current pool statistics for debugging.
    /// </summary>
    public void LogPoolStatistics()
    {
        try
        {
            var stats = GetPoolStatistics();
            _logger.LogInformation(
                "Connection Pool Statistics - Active: {Active}, Idle: {Idle}, Total: {Total}",
                stats.Active,
                stats.Idle,
                stats.Total);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log pool statistics");
        }
    }
}
