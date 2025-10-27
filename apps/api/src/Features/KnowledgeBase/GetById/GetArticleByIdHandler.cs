using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.KnowledgeBase.GetById;

public record GetArticleByIdQuery(Guid ArticleId, bool IncrementViewCount = false) : IRequest<ArticleDto?>;

public class GetArticleByIdHandler : IRequestHandler<GetArticleByIdQuery, ArticleDto?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetArticleByIdHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArticleDto?> Handle(GetArticleByIdQuery query, CancellationToken cancellationToken)
    {
        var article = await _dbContext.KnowledgeArticles
            .Include(a => a.Author)
            .Include(a => a.LastUpdatedBy)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Id == query.ArticleId, cancellationToken);

        if (article == null)
        {
            return null;
        }

        // Increment view count if requested (typically for public views, not previews)
        if (query.IncrementViewCount)
        {
            article.ViewCount++;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(article);
    }

    private static ArticleDto MapToDto(KnowledgeArticle article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            Status = article.Status.ToString(),
            CategoryId = article.CategoryId,
            CategoryName = article.Category?.Name,
            Tags = article.ArticleTags.Select(at => at.Tag.Name).ToList(),
            ViewCount = article.ViewCount,
            HelpfulCount = article.HelpfulCount,
            NotHelpfulCount = article.NotHelpfulCount,
            AuthorId = article.AuthorId,
            AuthorName = $"{article.Author.FirstName} {article.Author.LastName}",
            LastUpdatedById = article.LastUpdatedById,
            LastUpdatedByName = article.LastUpdatedBy != null 
                ? $"{article.LastUpdatedBy.FirstName} {article.LastUpdatedBy.LastName}" 
                : null,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            PublishedAt = article.PublishedAt
        };
    }
}
