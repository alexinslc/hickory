using System.Text;
using AspNetCoreRateLimit;
using FluentValidation;
using Hickory.Api.Common.Services;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Behaviors;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Messaging;
using Hickory.Api.Infrastructure.Middleware;
using Hickory.Api.Infrastructure.Notifications;
using Hickory.Api.Infrastructure.RealTime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

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

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    ));

// Authentication Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Business Services
builder.Services.AddScoped<ITicketNumberGenerator, TicketNumberGenerator>();

// Notification Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddHttpClient("webhooks", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// MediatR with pipeline behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
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

// Rate Limiting Configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

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
        tags: new[] { "ready", "db", "sql" });

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("hickory-api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("Hickory.Api"));

builder.Services.AddControllers();

// SignalR for real-time notifications
builder.Services.AddSignalR();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Hickory Help Desk API",
        Version = "v1",
        Description = "API for Hickory Help Desk System",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Hickory Support",
            Email = "support@hickory.dev"
        }
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Rate Limiting - placed before authentication to prevent unauthorized requests from consuming quotas
app.UseIpRateLimiting();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hickory API v1");
    options.RoutePrefix = "api-docs";
    options.DocumentTitle = "Hickory Help Desk API Documentation";
});

if (app.Environment.IsDevelopment())
{
    // Additional development-only configurations
}

app.UseHttpsRedirection();

// CORS must be placed after UseRouting and before UseAuthentication
app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

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

// Seed database with admin user on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    await DbSeeder.SeedAdminUser(context, passwordHasher, logger);
}

app.Run();
