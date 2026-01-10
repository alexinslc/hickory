using Hickory.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hickory.Api.IntegrationTests.TestFixtures;

/// <summary>
/// Custom WebApplicationFactory for integration tests with Testcontainers
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("hickory_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString(),
                ["ConnectionStrings:Redis"] = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}",
                ["JWT:Secret"] = "test-secret-key-at-least-32-characters-long",
                ["JWT:Issuer"] = "hickory-test",
                ["JWT:Audience"] = "hickory-test",
                ["JWT:ExpirationMinutes"] = "60",
                ["SkipDbSeeder"] = "true" // Skip database seeding in tests
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove the real database context
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            
            // Add test database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        // Start containers
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();

        // Run migrations
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Helper method to get an admin token for integration tests
    /// </summary>
    public async Task<string> GetAdminTokenAsync()
    {
        using var scope = Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        // Use the same JWT settings from the test configuration
        var secret = configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = configuration["JWT:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var audience = configuration["JWT:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-admin-id"),
            new Claim(ClaimTypes.Name, "Test Admin"),
            new Claim(ClaimTypes.Email, "admin@test.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );
        
        var tokenHandler = new JwtSecurityTokenHandler();
        return await Task.FromResult(tokenHandler.WriteToken(token));
    }
}
