using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Infrastructure.Data.Entities;
using NpgsqlTypes;

namespace Hickory.Api.Features.KnowledgeBase;

/// <summary>
/// Shared helper methods for knowledge article operations
/// </summary>
public static class KnowledgeArticleHelpers
{
    /// <summary>
    /// Generates a PostgreSQL full-text search vector from article title and content
    /// </summary>
    /// <param name="title">Article title (weighted 2x for higher relevance)</param>
    /// <param name="content">Article content</param>
    /// <returns>NpgsqlTsVector for full-text search</returns>
    public static NpgsqlTsVector GenerateSearchVector(string title, string content)
    {
        // Use native weighting: title (A), content (B)
        var titleVector = NpgsqlTsVector.Parse(title).SetWeight(NpgsqlTsVector.LexemeWeight.A);
        var contentVector = NpgsqlTsVector.Parse(content).SetWeight(NpgsqlTsVector.LexemeWeight.B);
        return titleVector + contentVector;
    }

    /// <summary>
    /// Maps a KnowledgeArticle entity to a complete ArticleDto
    /// </summary>
    public static ArticleDto MapToDto(KnowledgeArticle article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            Status = article.Status.ToString(),
            CategoryId = article.CategoryId,
            CategoryName = article.Category?.Name,
            Tags = article.Tags.Select(t => t.Name).ToList(),
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

    /// <summary>
    /// Maps a KnowledgeArticle entity to a simplified ArticleListItemDto for list views
    /// </summary>
    public static ArticleListItemDto MapToListItemDto(KnowledgeArticle article)
    {
        return new ArticleListItemDto
        {
            Id = article.Id,
            Title = article.Title,
            Status = article.Status.ToString(),
            CategoryId = article.CategoryId,
            CategoryName = article.Category?.Name,
            Tags = article.Tags.Select(t => t.Name).ToList(),
            ViewCount = article.ViewCount,
            HelpfulCount = article.HelpfulCount,
            AuthorName = $"{article.Author.FirstName} {article.Author.LastName}",
            CreatedAt = article.CreatedAt,
            PublishedAt = article.PublishedAt
        };
    }
}
