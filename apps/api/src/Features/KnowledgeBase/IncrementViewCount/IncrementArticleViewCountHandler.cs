using Hickory.Api.Infrastructure.Caching;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.KnowledgeBase.IncrementViewCount;

/// <summary>
/// Command to increment the view count for a knowledge base article.
/// Uses an atomic database update to avoid race conditions under concurrent access.
/// </summary>
public record IncrementArticleViewCountCommand(Guid ArticleId) : IRequest<Unit>;

public class IncrementArticleViewCountHandler : IRequestHandler<IncrementArticleViewCountCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;

    public IncrementArticleViewCountHandler(ApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<Unit> Handle(IncrementArticleViewCountCommand command, CancellationToken cancellationToken)
    {
        // Use ExecuteUpdateAsync for an atomic increment that avoids race conditions.
        // This translates to: UPDATE "KnowledgeArticles" SET "ViewCount" = "ViewCount" + 1 WHERE "Id" = @id
        var rowsAffected = await _dbContext.KnowledgeArticles
            .Where(a => a.Id == command.ArticleId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    a => a.ViewCount,
                    a => a.ViewCount + 1),
                cancellationToken);

        if (rowsAffected == 0)
        {
            throw new KeyNotFoundException($"Article with ID {command.ArticleId} not found");
        }

        // Invalidate the cached article so the next read picks up the new view count
        await _cacheService.RemoveAsync(CacheKeys.Article(command.ArticleId), cancellationToken);

        return Unit.Value;
    }
}
