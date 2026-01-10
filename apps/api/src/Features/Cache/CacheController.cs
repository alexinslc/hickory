using Hickory.Api.Infrastructure.Caching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hickory.Api.Features.Cache;

[ApiController]
[Route("api/cache")]
[Authorize(Roles = "Admin")]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get cache statistics including hit rate and total keys
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<CacheStatistics>> GetStatistics(CancellationToken cancellationToken)
    {
        var stats = await _cacheService.GetStatisticsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Clear all ticket caches
    /// </summary>
    [HttpDelete("tickets")]
    public async Task<IActionResult> ClearTickets(CancellationToken cancellationToken)
    {
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllTicketsPattern(), cancellationToken);
        _logger.LogInformation("All ticket caches cleared by admin");
        return NoContent();
    }

    /// <summary>
    /// Clear all knowledge base article caches
    /// </summary>
    [HttpDelete("articles")]
    public async Task<IActionResult> ClearArticles(CancellationToken cancellationToken)
    {
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllArticlesPattern(), cancellationToken);
        _logger.LogInformation("All article caches cleared by admin");
        return NoContent();
    }

    /// <summary>
    /// Clear specific ticket cache
    /// </summary>
    [HttpDelete("tickets/{ticketId:guid}")]
    public async Task<IActionResult> ClearTicket(Guid ticketId, CancellationToken cancellationToken)
    {
        await _cacheService.RemoveAsync(CacheKeys.Ticket(ticketId), cancellationToken);
        _logger.LogInformation("Ticket cache cleared by admin: {TicketId}", ticketId);
        return NoContent();
    }

    /// <summary>
    /// Clear specific article cache
    /// </summary>
    [HttpDelete("articles/{articleId:guid}")]
    public async Task<IActionResult> ClearArticle(Guid articleId, CancellationToken cancellationToken)
    {
        await _cacheService.RemoveAsync(CacheKeys.Article(articleId), cancellationToken);
        _logger.LogInformation("Article cache cleared by admin: {ArticleId}", articleId);
        return NoContent();
    }

    /// <summary>
    /// Clear all caches (use with caution)
    /// </summary>
    [HttpDelete("all")]
    public async Task<IActionResult> ClearAll(CancellationToken cancellationToken)
    {
        await _cacheService.RemoveByPatternAsync("hickory:*", cancellationToken);
        _logger.LogWarning("ALL caches cleared by admin");
        return NoContent();
    }
}
