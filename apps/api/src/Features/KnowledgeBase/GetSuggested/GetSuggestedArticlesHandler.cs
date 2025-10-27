using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace Hickory.Api.Features.KnowledgeBase.GetSuggested;

public record GetSuggestedArticlesQuery(
    string? TicketTitle,
    string? TicketDescription,
    Guid? CategoryId,
    List<string>? Tags,
    int Limit = 5
) : IRequest<List<ArticleListItemDto>>;

public class GetSuggestedArticlesHandler : IRequestHandler<GetSuggestedArticlesQuery, List<ArticleListItemDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetSuggestedArticlesHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ArticleListItemDto>> Handle(GetSuggestedArticlesQuery query, CancellationToken cancellationToken)
    {
        var articlesQuery = _dbContext.KnowledgeArticles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Where(a => a.Status == ArticleStatus.Published)
            .AsQueryable();

        // Priority 1: Match by category
        if (query.CategoryId.HasValue)
        {
            var categoryArticles = await articlesQuery
                .Where(a => a.CategoryId == query.CategoryId.Value)
                .OrderByDescending(a => a.HelpfulCount)
                .ThenByDescending(a => a.ViewCount)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            if (categoryArticles.Count >= query.Limit)
            {
                return categoryArticles.Select(MapToListItemDto).ToList();
            }
        }

        // Priority 2: Match by tags
        if (query.Tags != null && query.Tags.Any())
        {
            var tagArticles = await articlesQuery
                .Where(a => a.ArticleTags.Any(at => query.Tags.Contains(at.Tag.Name)))
                .OrderByDescending(a => a.ArticleTags.Count(at => query.Tags.Contains(at.Tag.Name))) // More matching tags = higher priority
                .ThenByDescending(a => a.HelpfulCount)
                .ThenByDescending(a => a.ViewCount)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            if (tagArticles.Count >= query.Limit)
            {
                return tagArticles.Select(MapToListItemDto).ToList();
            }
        }

        // Priority 3: Full-text search on title and description
        if (!string.IsNullOrWhiteSpace(query.TicketTitle) || !string.IsNullOrWhiteSpace(query.TicketDescription))
        {
            var searchText = $"{query.TicketTitle} {query.TicketDescription}".Trim();
            
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchVector = NpgsqlTsQuery.Parse(searchText);
                
                var searchArticles = await articlesQuery
                    .Where(a => a.SearchVector.Matches(searchVector))
                    .OrderByDescending(a => a.SearchVector.Rank(searchVector))
                    .ThenByDescending(a => a.HelpfulCount)
                    .Take(query.Limit)
                    .ToListAsync(cancellationToken);

                if (searchArticles.Any())
                {
                    return searchArticles.Select(MapToListItemDto).ToList();
                }
            }
        }

        // Priority 4: Fallback to most helpful articles
        var popularArticles = await articlesQuery
            .OrderByDescending(a => a.HelpfulCount)
            .ThenByDescending(a => a.ViewCount)
            .ThenByDescending(a => a.PublishedAt)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        return popularArticles.Select(MapToListItemDto).ToList();
    }

    private static ArticleListItemDto MapToListItemDto(KnowledgeArticle article)
    {
        return new ArticleListItemDto
        {
            Id = article.Id,
            Title = article.Title,
            Status = article.Status.ToString(),
            CategoryId = article.CategoryId,
            CategoryName = article.Category?.Name,
            Tags = article.ArticleTags.Select(at => at.Tag.Name).ToList(),
            ViewCount = article.ViewCount,
            HelpfulCount = article.HelpfulCount,
            AuthorName = $"{article.Author.FirstName} {article.Author.LastName}",
            CreatedAt = article.CreatedAt,
            PublishedAt = article.PublishedAt
        };
    }
}
