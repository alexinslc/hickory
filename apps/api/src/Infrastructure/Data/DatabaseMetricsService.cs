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
    /// Retrieves pool statistics by querying PostgreSQL's pg_stat_activity view.
    /// </summary>
    private (int Active, int Idle, int Total) GetPoolStatistics()
    {
        try
        {
            // This query counts active, idle, and total connections for the current database.
            const string sql = @"
                SELECT
                    COUNT(*) FILTER (WHERE state = 'active') AS active,
                    COUNT(*) FILTER (WHERE state = 'idle')   AS idle,
                    COUNT(*)                                  AS total
                FROM pg_stat_activity
                WHERE datname = current_database();";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = new NpgsqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                var active = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetInt64(0));
                var idle = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetInt64(1));
                var total = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetInt64(2));

                return (active, idle, total);
            }

            // If the query returns no rows, fall back to zeros.
            return (0, 0, 0);
        }
        catch (Exception ex)
        {
            // If statistics cannot be retrieved (e.g., insufficient permissions,
            // unsupported PostgreSQL version, or connectivity issues), log and
            // return zeros so callers can safely handle the failure.
            _logger.LogWarning(ex, "Failed to retrieve pool statistics from pg_stat_activity");
            return (0, 0, 0);
        }
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
