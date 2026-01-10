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
}
