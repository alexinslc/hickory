# Database Connection Pooling and Resilience

This directory contains infrastructure for database connection pooling, resilience patterns, and monitoring.

## Components

### DatabaseOptions
Configuration model for connection pooling and resilience settings. Configure via `appsettings.json`:

```json
{
  "Database": {
    "MinPoolSize": 2,
    "MaxPoolSize": 20,
    "ConnectionLifetimeSeconds": 600,
    "ConnectionIdleLifetimeSeconds": 300,
    "ConnectionTimeoutSeconds": 15,
    "CommandTimeoutSeconds": 30,
    "EnablePooling": true,
    "EnableRetryPolicy": true,
    "RetryCount": 3,
    "RetryDelaySeconds": 2,
    "EnableCircuitBreaker": true,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 60
  }
}
```

### DatabaseResilienceService
Provides retry logic and circuit breaker pattern for transient database failures.

**Usage Example - Repository Pattern:**

```csharp
public class UserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseResilienceService _resilience;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        ApplicationDbContext context,
        DatabaseResilienceService resilience,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _resilience = resilience;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _resilience.ExecuteAsync(
            async (cancellationToken) =>
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            },
            ct);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _resilience.ExecuteAsync(
            async (cancellationToken) =>
            {
                var changes = await _context.SaveChangesAsync(cancellationToken);
                return changes > 0;
            },
            ct);
    }
}
```

**Usage Example - MediatR Handler:**

```csharp
public class CreateTicketHandler : IRequestHandler<CreateTicketCommand, TicketDto>
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseResilienceService _resilience;

    public CreateTicketHandler(
        ApplicationDbContext context,
        DatabaseResilienceService resilience)
    {
        _context = context;
        _resilience = resilience;
    }

    public async Task<TicketDto> Handle(
        CreateTicketCommand request,
        CancellationToken cancellationToken)
    {
        return await _resilience.ExecuteAsync(async (ct) =>
        {
            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Status = TicketStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync(ct);

            return new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority,
                Status = ticket.Status
            };
        }, cancellationToken);
    }
}
```

**Usage Example - Raw SQL Operations:**

```csharp
public class DatabaseMaintenanceService
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseResilienceService _resilience;

    public async Task VacuumTablesAsync(CancellationToken ct = default)
    {
        await _resilience.ExecuteAsync(async (cancellationToken) =>
        {
            await _context.Database.ExecuteSqlRawAsync(
                "VACUUM ANALYZE tickets;",
                cancellationToken);
        }, ct);
    }
}
```

### DatabaseMetricsService
Tracks connection pool usage and performance metrics via OpenTelemetry.

**Available Metrics:**
- `db.connection.opened` - Counter for opened connections
- `db.connection.closed` - Counter for closed connections
- `db.connection.failed` - Counter for failed connection attempts
- `db.connection.open.duration` - Histogram of connection open times
- `db.pool.connections.active` - Gauge for active connections (from pg_stat_activity)
- `db.pool.connections.idle` - Gauge for idle connections (from pg_stat_activity)
- `db.pool.connections.total` - Gauge for total connections (from pg_stat_activity)

**Connection Event Tracking:**

To track connection lifecycle events, create a DbConnection interceptor:

```csharp
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Diagnostics;

public class ConnectionMetricsInterceptor : DbConnectionInterceptor
{
    private readonly DatabaseMetricsService _metrics;
    private readonly ConcurrentDictionary<int, Stopwatch> _connectionTimers = new();

    public ConnectionMetricsInterceptor(DatabaseMetricsService metrics)
    {
        _metrics = metrics;
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        var timer = Stopwatch.StartNew();
        _connectionTimers[connection.GetHashCode()] = timer;
        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (_connectionTimers.TryRemove(connection.GetHashCode(), out var timer))
        {
            timer.Stop();
            _metrics.RecordConnectionOpened(timer.Elapsed.TotalMilliseconds);
        }

        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override async Task ConnectionFailedAsync(
        DbConnection connection,
        ConnectionErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _connectionTimers.TryRemove(connection.GetHashCode(), out _);
        _metrics.RecordConnectionFailed();
        await base.ConnectionFailedAsync(connection, eventData, cancellationToken);
    }

    public override async Task ConnectionClosedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        _metrics.RecordConnectionClosed();
        await base.ConnectionClosedAsync(connection, eventData);
    }
}
```

Then register it in `Program.cs`:

```csharp
builder.Services.AddSingleton<ConnectionMetricsInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    // ... existing configuration ...
    
    var connectionInterceptor = serviceProvider.GetRequiredService<ConnectionMetricsInterceptor>();
    options.AddInterceptors(connectionInterceptor);
});
```

### DatabasePoolHealthCheck
Monitors connection pool health and configuration. Available at `/health/ready` endpoint.

**Health Check Data:**
- `pooling_enabled` - Whether connection pooling is enabled
- `min_pool_size` - Minimum pool size
- `max_pool_size` - Maximum pool size
- `connection_lifetime_sec` - Connection lifetime in seconds
- `idle_lifetime_sec` - Idle connection lifetime in seconds
- `retry_policy_enabled` - Whether retry policy is enabled
- `circuit_breaker_enabled` - Whether circuit breaker is enabled

**Status:**
- `Healthy` - Pool is properly configured and connections can be established
- `Degraded` - Pool size may be too small for production (< 10)
- `Unhealthy` - Cannot connect to database

## Transient Error Handling

The resilience service automatically retries the following PostgreSQL error codes:

| Error Code | Description |
|------------|-------------|
| 08000 | connection_exception |
| 08003 | connection_does_not_exist |
| 08006 | connection_failure |
| 40001 | serialization_failure |
| 40P01 | deadlock_detected |
| 53000 | insufficient_resources |
| 53100 | disk_full |
| 53200 | out_of_memory |
| 53300 | too_many_connections |
| 53400 | configuration_limit_exceeded |
| 57P03 | cannot_connect_now |
| 58000 | system_error |
| 58030 | io_error |

Non-transient errors (constraint violations, syntax errors, etc.) are not retried and will immediately throw.

## Circuit Breaker Behavior

The circuit breaker protects against cascading failures:

1. **Closed** (Normal) - All requests pass through
2. **Open** (Tripped) - All requests immediately fail without attempting database operations
3. **Half-Open** (Testing) - One request is allowed to test if the database has recovered

The circuit opens when:
- 50% of requests fail (configurable FailureRatio)
- At least 5 requests have been made in the sampling period (configurable MinimumThroughput)
- All failures are transient errors

The circuit remains open for 60 seconds (configurable) before entering Half-Open state.

## Production Recommendations

### Pool Sizing
- **Development**: 10-20 connections
- **Light Load** (< 100 concurrent users): 20-30 connections
- **Medium Load** (100-1000 users): 30-50 connections
- **Heavy Load** (> 1000 users): 50-100 connections

### Monitoring Alerts
Set up alerts for:
- Pool utilization > 80% - Consider increasing pool size
- High connection failure rate - Database availability issues
- Circuit breaker openings - Persistent database problems
- Connection open duration spikes - Slow queries or network issues

### Performance Tuning
Connection string optimizations already configured:
- `NoResetOnClose=true` - Faster connection reuse
- `MaxAutoPrepare=10` - Automatic statement preparation
- `AutoPrepareMinUsages=2` - Prepare frequently used queries

## Testing Resilience

Test the resilience policies by simulating failures:

```bash
# Test retry behavior - Kill database briefly
docker stop postgres-container
sleep 2
docker start postgres-container

# Test circuit breaker - Sustained outage
docker stop postgres-container
# Make multiple requests - circuit should open
# Wait 60 seconds - circuit should half-open
docker start postgres-container
```

## Migration from Non-Resilient Code

**Before:**
```csharp
public async Task<Ticket> GetTicketAsync(int id)
{
    return await _context.Tickets.FindAsync(id);
}
```

**After:**
```csharp
public async Task<Ticket> GetTicketAsync(int id, CancellationToken ct = default)
{
    return await _resilience.ExecuteAsync(
        async (cancellationToken) => await _context.Tickets.FindAsync(new object[] { id }, cancellationToken),
        ct);
}
```

**Key Changes:**
1. Inject `DatabaseResilienceService`
2. Wrap database operations with `_resilience.ExecuteAsync()`
3. Pass CancellationToken through
4. Lambda receives CancellationToken parameter

## Future Enhancements

- [ ] Add connection pool dashboard
- [ ] Implement automatic pool size tuning based on metrics
- [ ] Add alerting for pool exhaustion
- [ ] Separate read/write pools for database replicas
- [ ] Add distributed tracing for resilience operations
