using Hickory.Api.Features.KnowledgeBase.Create;
using Hickory.Api.Features.KnowledgeBase.GetById;
using Hickory.Api.Features.KnowledgeBase.GetSuggested;
using Hickory.Api.Features.KnowledgeBase.IncrementViewCount;
using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Features.KnowledgeBase.Rate;
using Hickory.Api.Features.KnowledgeBase.Search;
using Hickory.Api.Features.KnowledgeBase.Update;
using Hickory.Api.Infrastructure.Caching;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hickory.Api.Features.KnowledgeBase;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/knowledge")]
[Authorize]
public class KnowledgeController : ControllerBase
{
    private const string PublishedStatus = "Published";
    private static readonly TimeSpan ViewCountRateLimitWindow = TimeSpan.FromMinutes(5);

    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;

    public KnowledgeController(IMediator mediator, ICacheService cacheService)
    {
        _mediator = mediator;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Search and list knowledge base articles
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<SearchArticlesResult>> SearchArticles(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] List<string>? tags,
        [FromQuery] ArticleStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Only allow filtering by non-Published status for agents/admins
        if (status.HasValue && status.Value != ArticleStatus.Published)
        {
            var userRole = GetUserRole();
            if (userRole != AuthorizationRoles.Agent && userRole != AuthorizationRoles.Administrator)
            {
                return Forbid();
            }
        }

        var query = new SearchArticlesQuery(
            search,
            categoryId,
            tags,
            status,
            page,
            pageSize
        );

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific article by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ArticleDto>> GetArticleById(
        Guid id,
        [FromQuery] bool incrementViewCount = true,
        CancellationToken cancellationToken = default)
    {
        // First, get article without incrementing view count to check authorization
        var query = new GetArticleByIdQuery(id, IncrementViewCount: false);
        var article = await _mediator.Send(query, cancellationToken);

        if (article == null)
        {
            return NotFound();
        }

        // Only allow viewing non-Published articles for agents/admins
        if (article.Status != PublishedStatus)
        {
            var userRole = GetUserRole();
            if (userRole != AuthorizationRoles.Agent && userRole != AuthorizationRoles.Administrator)
            {
                return NotFound(); // Return 404 to avoid leaking existence
            }
        }

        // Increment view count if not recently viewed by the same client.
        // Uses a per-(article, IP) cache key with a short TTL to de-duplicate rapid repeat views.
        if (incrementViewCount)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var rateLimitKey = $"hickory:article-view:{id}:{clientIp}";
            var recentlyViewed = await _cacheService.GetAsync<ViewCountMarker>(rateLimitKey, cancellationToken);

            if (recentlyViewed == null)
            {
                await _cacheService.SetAsync(rateLimitKey, new ViewCountMarker(), ViewCountRateLimitWindow, cancellationToken);

                try
                {
                    await _mediator.Send(new IncrementArticleViewCountCommand(id), cancellationToken);
                }
                catch (Exception)
                {
                    // View count increment failure should not break the article GET response
                }
            }
        }

        return Ok(article);
    }

    /// <summary>
    /// Create a new knowledge base article (Agent/Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.AgentOrAdministrator)]
    public async Task<ActionResult<ArticleDto>> CreateArticle(
        [FromBody] CreateArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new CreateArticleCommand(request, userId);
        var article = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetArticleById),
            new { id = article.Id },
            article);
    }

    /// <summary>
    /// Update an existing knowledge base article (Agent/Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = AuthorizationRoles.AgentOrAdministrator)]
    public async Task<ActionResult<ArticleDto>> UpdateArticle(
        Guid id,
        [FromBody] UpdateArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        try
        {
            var command = new UpdateArticleCommand(id, request, userId);
            var article = await _mediator.Send(command, cancellationToken);
            return Ok(article);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Rate an article as helpful or not helpful
    /// </summary>
    [HttpPost("{id}/rate")]
    public async Task<IActionResult> RateArticle(
        Guid id,
        [FromBody] RateArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RateArticleCommand(id, request.IsHelpful);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get suggested articles based on ticket context
    /// </summary>
    [HttpGet("suggested")]
    public async Task<ActionResult<List<ArticleListItemDto>>> GetSuggestedArticles(
        [FromQuery] string? ticketTitle,
        [FromQuery] string? ticketDescription,
        [FromQuery] Guid? categoryId,
        [FromQuery] List<string>? tags,
        [FromQuery] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSuggestedArticlesQuery(
            ticketTitle,
            ticketDescription,
            categoryId,
            tags,
            limit
        );

        var articles = await _mediator.Send(query, cancellationToken);
        return Ok(articles);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        return userId;
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value
            ?? User.FindFirst("role")?.Value
            ?? "User";
    }
}

/// <summary>
/// Marker class used as a cache value to track recent article views for rate-limiting.
/// </summary>
public class ViewCountMarker
{
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
