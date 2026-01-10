# Health Check Endpoints

The Hickory API includes comprehensive health check endpoints for monitoring application and dependency health in production environments.

## Endpoints

### `/health`
**Purpose**: Overall application health  
**Response**: Returns `200 OK` with "Healthy" when all dependencies are operational  
**Checks**: All registered health checks (database, Redis)

```bash
curl http://localhost:5091/health
# Response: Healthy
```

### `/health/ready`
**Purpose**: Kubernetes/Docker readiness probe  
**Response**: Returns `200 OK` when the application is ready to accept traffic  
**Checks**: 
- PostgreSQL database connection
- Redis cache connection

```bash
curl http://localhost:5091/health/ready
# Response: Healthy
```

### `/health/live`
**Purpose**: Kubernetes/Docker liveness probe  
**Response**: Returns `200 OK` if the application process is running  
**Checks**: Basic application responsiveness (no dependency checks)

```bash
curl http://localhost:5091/health/live
# Response: Healthy
```

## Health Check Configuration

Health checks are configured in `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { "ready", "db" })
    .AddNpgSql(connectionString, name: "postgres", tags: new[] { "ready", "db", "sql" })
    .AddRedis(redisConnectionString, name: "redis", tags: new[] { "ready", "cache" }, timeout: TimeSpan.FromSeconds(5));
```

## Dependencies Monitored

| Dependency | Check Name | Timeout | Tags |
|------------|------------|---------|------|
| PostgreSQL (DbContext) | database | Default | ready, db |
| PostgreSQL (Direct) | postgres | Default | ready, db, sql |
| Redis | redis | 5 seconds | ready, cache |

## Production Deployment

### Docker Compose
Health checks are already configured in `docker/docker-compose.yml`:

```yaml
healthcheck:
  test: ["CMD", "wget", "--no-verbose", "--tries=1", "--spider", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

### Kubernetes
Use readiness and liveness probes:

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
  timeoutSeconds: 5
  failureThreshold: 3
```

## Monitoring and Alerting

Health check endpoints can be integrated with:
- **Prometheus**: Use blackbox exporter to scrape endpoints
- **Kubernetes**: Automatic pod restart on failed liveness probes
- **Docker**: Container health status visible in `docker ps`
- **Load Balancers**: Route traffic only to healthy instances
- **Uptime Monitoring**: External services (Pingdom, UptimeRobot, etc.)

## Troubleshooting

### Database Health Check Fails
- Verify PostgreSQL is running: `docker ps | grep postgres`
- Check connection string in `appsettings.json`
- Ensure migrations are applied: `dotnet ef database update`
- Check database logs: `docker logs hickory-postgres`

### Redis Health Check Fails
- Verify Redis is running: `docker ps | grep redis`
- Test Redis connection: `redis-cli -h localhost ping`
- Check Redis connection string in `appsettings.json`
- Ensure Redis port 6379 is accessible

### Slow Health Checks
- Redis timeout is set to 5 seconds
- Database queries should complete in <1 second
- Check network latency between services
- Review database and Redis performance metrics

## Testing

Run integration tests to verify health checks:

```bash
cd apps/api/Hickory.Api.IntegrationTests
dotnet test --filter FullyQualifiedName~HealthCheckTests
```

## See Also

- [ASP.NET Core Health Checks Documentation](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [AspNetCore.HealthChecks.Redis](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
- [Docker Health Check Documentation](https://docs.docker.com/engine/reference/builder/#healthcheck)
- [Kubernetes Probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
