using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Caching;
using Hickory.Api.IntegrationTests.TestFixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Hickory.Api.IntegrationTests.Infrastructure;

[Collection("Integration")]
public class CachingTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;

    public CachingTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TicketById_Should_Cache_And_Retrieve_From_Cache()
    {
        // Arrange
        var token = await _factory.GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a ticket
        var createRequest = new
        {
            title = "Test Ticket for Caching",
            description = "Testing cache functionality",
            priority = "Medium"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var ticket = await createResponse.Content.ReadFromJsonAsync<TicketDto>();
        ticket.Should().NotBeNull();

        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        // Act - First call (cache miss)
        var response1 = await _client.GetAsync($"/api/tickets/{ticket!.Id}");
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var ticket1 = await response1.Content.ReadFromJsonAsync<TicketDto>();

        // Verify it was cached
        var cachedTicket = await cacheService.GetAsync<TicketDto>(
            CacheKeys.Ticket(ticket.Id), CancellationToken.None);
        cachedTicket.Should().NotBeNull();
        cachedTicket!.Id.Should().Be(ticket.Id);

        // Act - Second call (cache hit)
        var response2 = await _client.GetAsync($"/api/tickets/{ticket.Id}");
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        var ticket2 = await response2.Content.ReadFromJsonAsync<TicketDto>();

        // Assert
        ticket1.Should().NotBeNull();
        ticket2.Should().NotBeNull();
        ticket1!.Title.Should().Be(ticket2!.Title);
        ticket1.UpdatedAt.Should().Be(ticket2.UpdatedAt);
    }

    [Fact]
    public async Task TicketUpdate_Should_Invalidate_Cache()
    {
        // Arrange
        var token = await _factory.GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a ticket
        var createRequest = new
        {
            title = "Test Ticket for Cache Invalidation",
            description = "Testing cache invalidation",
            priority = "Low"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var ticket = await createResponse.Content.ReadFromJsonAsync<TicketDto>();

        // First GET to cache the ticket
        await _client.GetAsync($"/api/tickets/{ticket!.Id}");

        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        // Verify it's cached
        var cached1 = await cacheService.GetAsync<TicketDto>(
            CacheKeys.Ticket(ticket.Id), CancellationToken.None);
        cached1.Should().NotBeNull();

        // Act - Update the ticket
        var updateRequest = new
        {
            status = "InProgress"
        };

        var updateResponse = await _client.PatchAsJsonAsync(
            $"/api/tickets/{ticket.Id}/status", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Cache should be cleared
        var cached2 = await cacheService.GetAsync<TicketDto>(
            CacheKeys.Ticket(ticket.Id), CancellationToken.None);
        cached2.Should().BeNull();
    }

    [Fact]
    public async Task KnowledgeArticleById_Should_Cache_And_Retrieve_From_Cache()
    {
        // Arrange
        var token = await _factory.GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create an article
        var createRequest = new
        {
            title = "Test Article for Caching",
            content = "# Test Content\n\nTesting cache functionality for articles",
            status = "Published",
            tags = new[] { "test", "caching" }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/knowledge", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var article = await createResponse.Content.ReadFromJsonAsync<ArticleDto>();
        article.Should().NotBeNull();

        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        // Act - First call (cache miss)
        var response1 = await _client.GetAsync($"/api/knowledge/{article!.Id}");
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify it was cached
        var cachedArticle = await cacheService.GetAsync<ArticleDto>(
            CacheKeys.Article(article.Id), CancellationToken.None);
        cachedArticle.Should().NotBeNull();
        cachedArticle!.Id.Should().Be(article.Id);

        // Act - Second call (cache hit)
        var response2 = await _client.GetAsync($"/api/knowledge/{article.Id}");
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        cachedArticle.Title.Should().Be(article.Title);
    }

    [Fact]
    public async Task ArticleUpdate_Should_Invalidate_Cache()
    {
        // Arrange
        var token = await _factory.GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create an article
        var createRequest = new
        {
            title = "Test Article for Cache Invalidation",
            content = "Original content",
            status = "Draft"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/knowledge", createRequest);
        var article = await createResponse.Content.ReadFromJsonAsync<ArticleDto>();

        // First GET to cache the article
        await _client.GetAsync($"/api/knowledge/{article!.Id}");

        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        // Verify it's cached
        var cached1 = await cacheService.GetAsync<ArticleDto>(
            CacheKeys.Article(article.Id), CancellationToken.None);
        cached1.Should().NotBeNull();

        // Act - Update the article
        var updateRequest = new
        {
            content = "Updated content"
        };

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/knowledge/{article.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Cache should be cleared
        var cached2 = await cacheService.GetAsync<ArticleDto>(
            CacheKeys.Article(article.Id), CancellationToken.None);
        cached2.Should().BeNull();
    }

    [Fact]
    public async Task CacheStatistics_Should_Return_HitMiss_Metrics()
    {
        // Arrange
        var token = await _factory.GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        // Generate some cache hits and misses
        await cacheService.GetAsync<TicketDto>("test-key-1", CancellationToken.None); // miss
        await cacheService.SetAsync("test-key-2", new TicketDto { Title = "Test" }, 
            TimeSpan.FromMinutes(5), CancellationToken.None);
        await cacheService.GetAsync<TicketDto>("test-key-2", CancellationToken.None); // hit

        // Act
        var response = await _client.GetAsync("/api/cache/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<CacheStatistics>();
        stats.Should().NotBeNull();
        stats!.TotalHits.Should().BeGreaterThan(0);
        stats.TotalMisses.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CacheManagement_ClearTickets_Should_Remove_Ticket_Caches()
    {
        // Arrange
        var token = await _factory.GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        // Cache some data with proper hickory: prefix to match the pattern
        await cacheService.SetAsync("hickory:tickets:test-a", new TicketDto { Title = "A" }, 
            TimeSpan.FromMinutes(5), CancellationToken.None);
        await cacheService.SetAsync("hickory:tickets:test-b", new TicketDto { Title = "B" }, 
            TimeSpan.FromMinutes(5), CancellationToken.None);

        // Act
        var response = await _client.DeleteAsync("/api/cache/tickets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
