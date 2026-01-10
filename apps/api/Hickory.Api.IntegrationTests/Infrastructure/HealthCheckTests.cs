using System.Net;
using System.Text.Json;
using FluentAssertions;
using Hickory.Api.IntegrationTests.TestFixtures;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hickory.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Integration tests for health check endpoints
/// </summary>
public class HealthCheckTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HealthCheckTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoint_Should_Return_Healthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthReady_Endpoint_Should_Return_Healthy_With_All_Dependencies()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthLive_Endpoint_Should_Return_Healthy()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthReady_Should_Check_Database_Connection()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the database health check is registered
        using var scope = _factory.Services.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
        var result = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains("db"));
        
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Entries.Should().ContainKey("database");
        result.Entries.Should().ContainKey("postgres");
    }

    [Fact]
    public async Task HealthReady_Should_Check_Redis_Connection()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the Redis health check is registered
        using var scope = _factory.Services.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
        var result = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains("cache"));
        
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Entries.Should().ContainKey("redis");
        result.Entries["redis"].Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task Redis_HealthCheck_Should_Complete_Within_Timeout()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
        
        // Act
        var result = await healthCheckService.CheckHealthAsync(check => check.Name == "redis");

        // Assert
        result.Entries["redis"].Status.Should().Be(HealthStatus.Healthy);
        result.Entries["redis"].Duration.Should().BeLessThan(
            TimeSpan.FromSeconds(10),
            "the Redis health check should complete within its configured timeout");
    }

    [Fact]
    public async Task HealthReady_Should_Include_All_Required_Dependencies()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
        
        // Act
        var result = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains("ready"));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        
        // Should have database checks
        result.Entries.Should().ContainKey("database");
        result.Entries.Should().ContainKey("postgres");
        
        // Should have Redis check
        result.Entries.Should().ContainKey("redis");
        
        // All should be healthy
        result.Entries.Values.Should().OnlyContain(entry => entry.Status == HealthStatus.Healthy);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/health/live")]
    public async Task HealthCheck_Endpoints_Should_Be_Accessible_Without_Authentication(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized, 
            "health check endpoints should be accessible without authentication");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_Should_Include_Detailed_Information_In_Response()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
        
        // Act
        var result = await healthCheckService.CheckHealthAsync();

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        
        // Verify detailed information is available
        foreach (var entry in result.Entries)
        {
            entry.Value.Should().NotBeNull();
            entry.Value.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
            entry.Value.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        }
    }
}
