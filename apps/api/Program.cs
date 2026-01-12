using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Hickory.Api.Common.Services;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Behaviors;
using Hickory.Api.Infrastructure.Caching;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Health;
using Hickory.Api.Infrastructure.Messaging;
using Hickory.Api.Infrastructure.Middleware;
using Hickory.Api.Infrastructure.Notifications;
using Hickory.Api.Infrastructure.RealTime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/hickory-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

// HttpContext accessor for audit logging
builder.Services.AddHttpContextAccessor();

// Database configuration and resilience
var databaseOptions = new DatabaseOptions();
builder.Configuration.GetSection(DatabaseOptions.SectionName).Bind(databaseOptions);
builder.Services.AddSingleton(databaseOptions);
builder.Services.AddSingleton<DatabaseResilienceService>();
builder.Services.AddSingleton<DatabaseMetricsService>();

// Register QueryPerformanceInterceptor as singleton for reuse across all DbContext instances
builder.Services.AddSingleton<QueryPerformanceInterceptor>();

// Database with connection pooling and resilience
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var dbOptions = serviceProvider.GetRequiredService<DatabaseOptions>();
    
    // Build connection string with pooling parameters
    var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder(baseConnectionString)
    {
        Pooling = dbOptions.EnablePooling,
        MinPoolSize = dbOptions.MinPoolSize,
        MaxPoolSize = dbOptions.MaxPoolSize,
        ConnectionLifetime = dbOptions.ConnectionLifetimeSeconds,
        ConnectionIdleLifetime = dbOptions.ConnectionIdleLifetimeSeconds,
        Timeout = dbOptions.ConnectionTimeoutSeconds,
        CommandTimeout = dbOptions.CommandTimeoutSeconds,
        // Additional recommended settings for production
        NoResetOnClose = true, // Improves performance by not resetting connection state
        MaxAutoPrepare = 10, // Auto-prepare frequently used statements
        AutoPrepareMinUsages = 2 // Prepare statements used at least twice
    };
    
    options.UseNpgsql(
        connectionStringBuilder.ConnectionString,
        npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    );
    
    // Add query performance interceptor for metrics - reuse singleton instance
    var interceptor = serviceProvider.GetRequiredService<QueryPerformanceInterceptor>();
    options.AddInterceptors(interceptor);
    
    // Enable detailed logging in development only.
    // WARNING: Do NOT enable EnableSensitiveDataLogging in production as it will log parameter values,
    // including potentially sensitive user data (emails, names, passwords, etc.).
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging(); // Shows parameter values in logs
        options.EnableDetailedErrors(); // More detailed error messages
        options.LogTo(
            message => Log.Debug(message),
            new[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting },
            Microsoft.Extensions.Logging.LogLevel.Information
        );
    }
});

// Redis Distributed Cache
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.AbortOnConnectFail = false; // Don't fail startup if Redis is temporarily unavailable
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    // Note: InstanceName removed to avoid duplicate prefixing with CacheKeys class
});

// Caching Services - Singleton to maintain statistics across all requests
builder.Services.AddSingleton<ICacheService, CacheService>();

// Authentication Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<Hickory.Api.Features.Auth.TwoFactor.ITwoFactorService, 
    Hickory.Api.Features.Auth.TwoFactor.TwoFactorService>();

// Business Services
builder.Services.AddScoped<ITicketNumberGenerator, TicketNumberGenerator>();

// File Storage
builder.Services.Configure<Hickory.Api.Infrastructure.Storage.LocalFileStorageOptions>(
    builder.Configuration.GetSection("FileStorage"));
builder.Services.AddSingleton<Hickory.Api.Infrastructure.Storage.IFileStorageService, 
    Hickory.Api.Infrastructure.Storage.LocalFileStorageService>();

// Notification Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddHttpClient("webhooks", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Audit Logging
builder.Services.AddScoped<Hickory.Api.Infrastructure.Audit.IAuditLogService, 
    Hickory.Api.Infrastructure.Audit.AuditLogService>();

// MediatR with pipeline behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(AuditingBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// MassTransit for event-driven messaging
builder.Services.AddMessaging(builder.Configuration);

// JWT Authentication
var jwtSecret = builder.Configuration["JWT:Secret"] 
    ?? throw new InvalidOperationException("JWT:Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Helper for rate limiting partition key with security logging
static string GetRateLimitPartitionKey(HttpContext context, string limitType, IServiceProvider? services = null)
{
    var userId = context.User?.FindFirst("sub")?.Value;
    var ipAddress = context.Connection.RemoteIpAddress?.ToString();
    var partitionKey = userId ?? ipAddress ?? "anonymous";
    
    // Log warning when falling back to "anonymous" (security concern)
    if (partitionKey == "anonymous")
    {
        var logger = (services ?? context.RequestServices).GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Rate limiting ({LimitType}): Unable to determine user ID or IP address. Using 'anonymous' partition key which may allow multiple users to share the same rate limit bucket.", limitType);
    }
    
    return partitionKey;
}

// Rate Limiting - protects API from abuse
builder.Services.AddRateLimiter(options =>
{
    // Global rate limit: 100 requests per minute per user (or IP for anonymous requests)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var partitionKey = GetRateLimitPartitionKey(context, "global");
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 100),
                Window = TimeSpan.FromMinutes(builder.Configuration.GetValue("RateLimiting:WindowMinutes", 1)),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 // No queueing, reject immediately
            });
    });
    
    // Stricter limit for authentication endpoints (prevent brute force), per user/IP
    options.AddPolicy("auth", context =>
    {
        var partitionKey = GetRateLimitPartitionKey(context, "auth");

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:AuthPermitLimit", 10),
                Window = TimeSpan.FromMinutes(builder.Configuration.GetValue("RateLimiting:AuthWindowMinutes", 1)),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
    
    // Custom response for rate limited requests
    options.OnRejected = async (context, cancellationToken) =>
    {
        // Check if response has already started before modifying it
        if (context.HttpContext.Response.HasStarted)
        {
            return;
        }
        
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
            ? retryAfterValue.TotalSeconds
            : 60;
            
        // RFC 7231: Retry-After header must be an integer representing seconds
        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter).ToString();
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            type = "https://httpstatuses.io/429",
            title = "Too Many Requests",
            status = 429,
            detail = $"Rate limit exceeded. Try again in {(int)retryAfter} seconds."
        }, cancellationToken);
    };
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { "ready", "db" })
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgres",
        tags: new[] { "ready", "db", "sql" })
    .AddCheck<DatabasePoolHealthCheck>(
        "database-pool",
        tags: new[] { "ready", "db", "pool" })
    .AddRedis(
        redisConnectionString,
        name: "redis",
        tags: new[] { "ready", "cache" },
        timeout: TimeSpan.FromSeconds(5));

// OpenTelemetry - Enhanced for .NET 10
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("hickory-api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("Hickory.Api"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Http")
        .AddMeter("Microsoft.AspNetCore.Authentication")
        .AddMeter("Hickory.Api")
        .AddMeter("Hickory.Api.Cache")
        .AddMeter("Hickory.Api.Database"));

builder.Services.AddControllers();

// SignalR for real-time notifications
builder.Services.AddSignalR();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

// Add built-in .NET 10 OpenAPI support for YAML format
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hickory Help Desk API",
        Version = "v1",
        Description = "API for Hickory Help Desk System with OpenAPI 3.1 support",
        Contact = new OpenApiContact
        {
            Name = "Hickory Support",
            Email = "support@hickory.dev"
        }
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below."
    });

    options.AddSecurityRequirement(document =>
    {
        return new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        };
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Security headers
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    
    // Content Security Policy - use appropriate policy based on route
    if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/api-docs", StringComparison.OrdinalIgnoreCase))
    {
        // Swagger UI needs to load JS, CSS, images, and fonts
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "object-src 'none'; " +
            "frame-ancestors 'none'");
    }
    else
    {
        // API returns JSON, restrict everything by default
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'none'; object-src 'none'; frame-ancestors 'none'");
    }
    
    // Prevent MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // Control iframe embedding
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // Control referrer information
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Enforce HTTPS
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    
    // Disable browser features
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    
    // Prevent caching of sensitive API responses (but allow caching for Swagger/OpenAPI/docs)
    if (!path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
        && !path.StartsWith("/api-docs", StringComparison.OrdinalIgnoreCase)
        && !path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Headers.Append("Cache-Control", "no-store");
        context.Response.Headers.Append("Pragma", "no-cache");
    }
    
    await next();
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hickory API v1 (JSON)");
    options.SwaggerEndpoint("/swagger/v1/swagger.yaml", "Hickory API v1 (YAML)");
    options.RoutePrefix = "api-docs";
    options.DocumentTitle = "Hickory Help Desk API Documentation";
});

// Map built-in .NET 10 OpenAPI endpoints (provides YAML support)
app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    // Additional development-only configurations
}

app.UseHttpsRedirection();

// CORS must be placed after UseRouting and before UseAuthentication
app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

// Rate limiting - after auth so we can use user identity for partitioning
app.UseRateLimiter();

// Health checks endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Just checks if the app is running
});

app.MapControllers();

// SignalR hub endpoint
app.MapHub<NotificationHub>("/hubs/notifications");

// Auto-migrate and seed database on startup (skip in test environments)
if (!builder.Configuration.GetValue<bool>("SkipDbSeeder"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        // Auto-migrate database in development/Docker (controlled by environment)
        if (builder.Configuration.GetValue<bool>("Database:AutoMigrate", defaultValue: false) ||
            builder.Environment.IsDevelopment())
        {
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        
        await DbSeeder.SeedAdminUser(context, passwordHasher, logger);
    }
}

app.Run();
