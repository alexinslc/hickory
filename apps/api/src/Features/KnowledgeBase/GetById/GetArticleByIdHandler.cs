using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Infrastructure.Caching;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.KnowledgeBase.GetById;

public record GetArticleByIdQuery(Guid ArticleId, bool IncrementViewCount = false) : IRequest<ArticleDto?>;

public class GetArticleByIdHandler : IRequestHandler<GetArticleByIdQuery, ArticleDto?>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;

    public GetArticleByIdHandler(ApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<ArticleDto?> Handle(GetArticleByIdQuery query, CancellationToken cancellationToken)
    {
        // If incrementing view count, skip cache and update database
        if (query.IncrementViewCount)
        {
            var article = await _dbContext.KnowledgeArticles
                .Include(a => a.Author)
                .Include(a => a.LastUpdatedBy)
                .Include(a => a.Category)
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.Id == query.ArticleId, cancellationToken);

            if (article == null)
            {
                return null;
            }

            article.ViewCount++;
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            var dto = KnowledgeArticleHelpers.MapToDto(article);
            
            // Update cache with new view count
            var cacheKey = CacheKeys.Article(query.ArticleId);
            await _cacheService.SetAsync(cacheKey, dto, CacheExpiration.KnowledgeArticles, cancellationToken);
            
            return dto;
        }

        // Try cache first for read-only requests
        var cachedArticle = await _cacheService.GetOrCreateAsync(
            CacheKeys.Article(query.ArticleId),
            async ct =>
            {
                var article = await _dbContext.KnowledgeArticles
                    .Include(a => a.Author)
                    .Include(a => a.LastUpdatedBy)
                    .Include(a => a.Category)
                    .Include(a => a.Tags)
                    .FirstOrDefaultAsync(a => a.Id == query.ArticleId, ct);

                return article != null ? KnowledgeArticleHelpers.MapToDto(article) : null!;
            },
            CacheExpiration.KnowledgeArticles,
            cancellationToken);

        return cachedArticle;
    }
}
