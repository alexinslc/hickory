# Database Connection Pooling Configuration

## Overview
This document describes the database connection pooling and resilience configuration implemented for optimal production performance and reliability.

## Features Implemented

### 1. Connection Pooling
Configured Npgsql connection pooling with production-optimized settings:
- **Min Pool Size**: 2 connections (maintained even when idle)
- **Max Pool Size**: 20 connections (configurable based on load)
- **Connection Lifetime**: 600 seconds (10 minutes)
- **Idle Lifetime**: 300 seconds (5 minutes)
- **Connection Timeout**: 15 seconds
- **Command Timeout**: 30 seconds

Additional performance settings:
- `NoResetOnClose`: true - Improves performance by avoiding connection state resets
- `MaxAutoPrepare`: 10 - Auto-prepares frequently used SQL statements
- `AutoPrepareMinUsages`: 2 - Prepares statements used at least twice

### 2. Resilience Policies (Polly)

#### Retry Policy
- Automatically retries transient database errors
- Exponential backoff with jitter
- Default: 3 retry attempts with 2-second base delay
- Only retries on known PostgreSQL transient errors:
  - Connection failures (08xxx)
  - Serialization failures (40001)
  - Deadlocks (40P01)
  - Resource exhaustion (53xxx)
  - Temporary unavailability (57P03)
  - System/IO errors (58xxx)

#### Circuit Breaker
- Prevents cascading failures during database outages
- Opens circuit after 50% failure rate with minimum 5 requests
- Remains open for 60 seconds before testing recovery
- Logs circuit state changes for monitoring

### 3. Metrics and Monitoring
Implemented comprehensive metrics for observability:
- `db.connection.opened` - Counter for opened connections
- `db.connection.closed` - Counter for closed connections
- `db.connection.failed` - Counter for failed connection attempts
- `db.connection.open.duration` - Histogram of connection open times
- `db.pool.connections.active` - Gauge for active connections
- `db.pool.connections.idle` - Gauge for idle connections
- `db.pool.connections.total` - Gauge for total pooled connections

### 4. Health Checks
Added `database-pool` health check endpoint:
- Validates connection can be established
- Reports pool configuration
- Warns if pool size is too small for production (<10)
- Available at `/health/ready` endpoint

## Configuration

### appsettings.json
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

### Production Recommendations

#### Pool Sizing
- **Light Load** (< 100 concurrent users): `MaxPoolSize: 10-20`
- **Medium Load** (100-1000 users): `MaxPoolSize: 20-50`
- **Heavy Load** (> 1000 users): `MaxPoolSize: 50-100`

Formula: `MaxPoolSize = (Number of processors) * 2 + Effective disk spindles`

#### Connection Lifetime
- Default 10 minutes is good for most scenarios
- Reduce to 5 minutes if you need faster connection refresh
- Increase to 30 minutes for very stable environments

#### Retry Configuration
- Increase `RetryCount` to 5 for environments with frequent transient errors
- Reduce `RetryDelaySeconds` to 1 for time-sensitive operations
- Disable retries for write operations that must not be duplicated

## Monitoring

### Health Check
```bash
curl http://localhost:5000/health/ready
```

Response includes:
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database-pool",
      "status": "Healthy",
      "data": {
        "pooling_enabled": true,
        "min_pool_size": 2,
        "max_pool_size": 20,
        "retry_policy_enabled": true,
        "circuit_breaker_enabled": true
      }
    }
  ]
}
```

### Logs
Watch for these log patterns:
- `Database operation failed with transient error` - Retry in progress
- `Database circuit breaker opened` - Database experiencing failures
- `Database circuit breaker closed` - Recovery complete

### Metrics (Prometheus/OpenTelemetry)
Available meters:
- `Hickory.Api.Database` - All database connection metrics

## Testing

### Load Testing
Use the provided performance tests to validate pool configuration:
```bash
cd tests/performance
./setup.sh
npm test
```

Monitor metrics during load test to ensure:
- Connection pool doesn't exhaust (< 80% of MaxPoolSize)
- No connection timeouts occur
- Retry policy handles transient errors gracefully

### Connection Leak Detection
Monitor `db.pool.connections.active` over time:
- Should return to near MinPoolSize during idle periods
- Constant growth indicates connection leaks

## Files Changed

### New Files
- `src/Infrastructure/Data/DatabaseOptions.cs` - Configuration model
- `src/Infrastructure/Data/DatabaseResilienceService.cs` - Retry and circuit breaker
- `src/Infrastructure/Data/DatabaseMetricsService.cs` - Connection pool metrics
- `src/Infrastructure/Health/DatabasePoolHealthCheck.cs` - Pool health check

### Modified Files
- `Program.cs` - Wired up pooling, resilience, and monitoring services
- `appsettings.json` - Added Database configuration section

## Related Issues
- Closes #68 - Database Connection Pooling
- Related to #64 - Health Check Endpoints (enhanced)

## Future Enhancements
- Add connection pool dashboard in monitoring system
- Implement automatic pool size tuning based on load
- Add alerting for pool exhaustion events
- Separate read/write connection pools for read replicas
