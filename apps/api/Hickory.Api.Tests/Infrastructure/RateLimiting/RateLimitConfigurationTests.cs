using AspNetCoreRateLimit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hickory.Api.Tests.Infrastructure.RateLimiting;

public class RateLimitConfigurationTests
{
    [Fact]
    public void RateLimitOptions_LoadsFromConfiguration_Production()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"IpRateLimiting:EnableEndpointRateLimiting", "true"},
            {"IpRateLimiting:StackBlockedRequests", "false"},
            {"IpRateLimiting:RealIpHeader", "X-Real-IP"},
            {"IpRateLimiting:ClientIdHeader", "X-ClientId"},
            {"IpRateLimiting:HttpStatusCode", "429"},
            {"IpRateLimiting:GeneralRules:0:Endpoint", "*"},
            {"IpRateLimiting:GeneralRules:0:Period", "1m"},
            {"IpRateLimiting:GeneralRules:0:Limit", "100"},
            {"IpRateLimiting:GeneralRules:1:Endpoint", "*"},
            {"IpRateLimiting:GeneralRules:1:Period", "15m"},
            {"IpRateLimiting:GeneralRules:1:Limit", "500"},
            {"IpRateLimiting:GeneralRules:2:Endpoint", "*"},
            {"IpRateLimiting:GeneralRules:2:Period", "1h"},
            {"IpRateLimiting:GeneralRules:2:Limit", "1000"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<IpRateLimitOptions>>().Value;

        // Assert
        options.Should().NotBeNull();
        options.EnableEndpointRateLimiting.Should().BeTrue();
        options.StackBlockedRequests.Should().BeFalse();
        options.RealIpHeader.Should().Be("X-Real-IP");
        options.ClientIdHeader.Should().Be("X-ClientId");
        options.HttpStatusCode.Should().Be(429);
        
        options.GeneralRules.Should().HaveCount(3);
        
        // Verify 1 minute limit
        options.GeneralRules[0].Endpoint.Should().Be("*");
        options.GeneralRules[0].Period.Should().Be("1m");
        options.GeneralRules[0].Limit.Should().Be(100);
        
        // Verify 15 minute limit
        options.GeneralRules[1].Endpoint.Should().Be("*");
        options.GeneralRules[1].Period.Should().Be("15m");
        options.GeneralRules[1].Limit.Should().Be(500);
        
        // Verify 1 hour limit
        options.GeneralRules[2].Endpoint.Should().Be("*");
        options.GeneralRules[2].Period.Should().Be("1h");
        options.GeneralRules[2].Limit.Should().Be(1000);
    }

    [Fact]
    public void RateLimitOptions_LoadsFromConfiguration_Development()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"IpRateLimiting:EnableEndpointRateLimiting", "true"},
            {"IpRateLimiting:StackBlockedRequests", "false"},
            {"IpRateLimiting:RealIpHeader", "X-Real-IP"},
            {"IpRateLimiting:ClientIdHeader", "X-ClientId"},
            {"IpRateLimiting:HttpStatusCode", "429"},
            {"IpRateLimiting:GeneralRules:0:Endpoint", "*"},
            {"IpRateLimiting:GeneralRules:0:Period", "1m"},
            {"IpRateLimiting:GeneralRules:0:Limit", "1000"},
            {"IpRateLimiting:GeneralRules:1:Endpoint", "*"},
            {"IpRateLimiting:GeneralRules:1:Period", "15m"},
            {"IpRateLimiting:GeneralRules:1:Limit", "5000"},
            {"IpRateLimiting:GeneralRules:2:Endpoint", "*"},
            {"IpRateLimiting:GeneralRules:2:Period", "1h"},
            {"IpRateLimiting:GeneralRules:2:Limit", "10000"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<IpRateLimitOptions>>().Value;

        // Assert
        options.Should().NotBeNull();
        options.EnableEndpointRateLimiting.Should().BeTrue();
        options.StackBlockedRequests.Should().BeFalse();
        
        options.GeneralRules.Should().HaveCount(3);
        
        // Verify development limits are more lenient
        options.GeneralRules[0].Limit.Should().Be(1000);
        options.GeneralRules[1].Limit.Should().Be(5000);
        options.GeneralRules[2].Limit.Should().Be(10000);
    }
}
