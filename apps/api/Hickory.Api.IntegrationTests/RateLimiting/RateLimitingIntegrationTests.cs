using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Hickory.Api.IntegrationTests.RateLimiting;

public class RateLimitingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RateLimitingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RateLimiting_ReturnsRateLimitHeaders_OnSuccessfulRequest()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify rate limit headers are present
        response.Headers.Should().ContainKey("X-Rate-Limit-Limit");
        response.Headers.Should().ContainKey("X-Rate-Limit-Remaining");
        response.Headers.Should().ContainKey("X-Rate-Limit-Reset");
        
        // Verify the limit value is set
        var limitHeader = response.Headers.GetValues("X-Rate-Limit-Limit").FirstOrDefault();
        limitHeader.Should().NotBeNullOrEmpty();
        
        var remainingHeader = response.Headers.GetValues("X-Rate-Limit-Remaining").FirstOrDefault();
        remainingHeader.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RateLimiting_DecrementsRemainingCount_OnMultipleRequests()
    {
        // This test verifies rate limiting behavior by making multiple rapid requests.
        // In development, the limit is 1000 requests per minute, so we'll make a smaller
        // number of requests and verify the remaining count decreases.
        
        // Arrange
        var requestCount = 10;
        int? initialRemaining = null;
        int? finalRemaining = null;

        // Act
        for (int i = 0; i < requestCount; i++)
        {
            var response = await _client.GetAsync("/health");
            
            if (i == 0)
            {
                // Capture initial remaining count
                var remainingHeader = response.Headers.GetValues("X-Rate-Limit-Remaining").FirstOrDefault();
                initialRemaining = int.Parse(remainingHeader ?? "0");
            }
            
            if (i == requestCount - 1)
            {
                // Capture final remaining count
                var remainingHeader = response.Headers.GetValues("X-Rate-Limit-Remaining").FirstOrDefault();
                finalRemaining = int.Parse(remainingHeader ?? "0");
            }
        }

        // Assert
        initialRemaining.Should().NotBeNull();
        finalRemaining.Should().NotBeNull();
        finalRemaining.Should().BeLessThan(initialRemaining!.Value);
        
        // Verify that the remaining count decreased by approximately the number of requests made
        (initialRemaining!.Value - finalRemaining!.Value).Should().BeGreaterOrEqualTo(requestCount - 1);
    }

    [Fact]
    public async Task RateLimiting_HeadersShowCorrectValues()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify limit header shows expected value (1000 for development)
        var limitHeader = response.Headers.GetValues("X-Rate-Limit-Limit").FirstOrDefault();
        var limit = int.Parse(limitHeader ?? "0");
        limit.Should().BeGreaterThan(0);
        
        // Verify remaining is less than or equal to limit
        var remainingHeader = response.Headers.GetValues("X-Rate-Limit-Remaining").FirstOrDefault();
        var remaining = int.Parse(remainingHeader ?? "0");
        remaining.Should().BeLessOrEqualTo(limit);
        
        // Verify reset header is present and is a valid timestamp
        var resetHeader = response.Headers.GetValues("X-Rate-Limit-Reset").FirstOrDefault();
        resetHeader.Should().NotBeNullOrEmpty();
        
        // The reset header should be parseable as a date/time
        DateTime.TryParse(resetHeader, out var resetTime).Should().BeTrue();
        resetTime.Should().BeAfter(DateTime.UtcNow);
    }
}
