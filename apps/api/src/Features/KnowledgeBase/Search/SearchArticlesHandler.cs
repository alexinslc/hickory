using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace Hickory.Api.Features.KnowledgeBase.Search;

public record SearchArticlesQuery(
    string? SearchQuery,
    Guid? CategoryId,
    List<string>? Tags,
    ArticleStatus? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<SearchArticlesResult>;

public class SearchArticlesHandler : IRequestHandler<SearchArticlesQuery, SearchArticlesResult>
{
    private readonly ApplicationDbContext _dbContext;

    public SearchArticlesHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchArticlesResult> Handle(SearchArticlesQuery query, CancellationToken cancellationToken)
    {
        var articlesQuery = _dbContext.KnowledgeArticles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .AsQueryable();

        // Apply status filter (default to Published for public searches)
        if (query.Status.HasValue)
        {
            articlesQuery = articlesQuery.Where(a => a.Status == query.Status.Value);
        }
        else
        {
            // Default to only published articles
            articlesQuery = articlesQuery.Where(a => a.Status == ArticleStatus.Published);
        }

        // Apply category filter
        if (query.CategoryId.HasValue)
        {
            articlesQuery = articlesQuery.Where(a => a.CategoryId == query.CategoryId.Value);
        }

        // Apply tag filter
        if (query.Tags != null && query.Tags.Any())
        {
            articlesQuery = articlesQuery.Where(a => 
                a.ArticleTags.Any(at => query.Tags.Contains(at.Tag.Name)));
        }

        // Apply full-text search if provided
        if (!string.IsNullOrWhiteSpace(query.SearchQuery))
        {
            var searchVector = NpgsqlTsQuery.Parse(query.SearchQuery);
            
            // Use PostgreSQL full-text search on the SearchVector column
            articlesQuery = articlesQuery
                .Where(a => a.SearchVector.Matches(searchVector))
                .OrderByDescending(a => a.SearchVector.Rank(searchVector));
        }
        else
        {
            // Default sort by most recent published date
            articlesQuery = articlesQuery.OrderByDescending(a => a.PublishedAt ?? a.CreatedAt);
        }

        // Get total count for pagination
        var totalCount = await articlesQuery.CountAsync(cancellationToken);

        // Apply pagination
        var skip = (query.Page - 1) * query.PageSize;
        var articles = await articlesQuery
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new SearchArticlesResult
        {
            Articles = articles.Select(MapToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };
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
