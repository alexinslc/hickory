using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Features.KnowledgeBase.Models;

/// <summary>
/// DTO for knowledge article responses
/// </summary>
public record ArticleDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public List<string> Tags { get; init; } = new();
    public int ViewCount { get; init; }
    public int HelpfulCount { get; init; }
    public int NotHelpfulCount { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public Guid? LastUpdatedById { get; init; }
    public string? LastUpdatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
}

/// <summary>
/// Simplified DTO for article list views
/// </summary>
public record ArticleListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public List<string> Tags { get; init; } = new();
    public int ViewCount { get; init; }
    public int HelpfulCount { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
}

/// <summary>
/// Request model for creating a new article
/// </summary>
public record CreateArticleRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid? CategoryId { get; init; }
    public List<string> Tags { get; init; } = new();
    public ArticleStatus Status { get; init; } = ArticleStatus.Draft;
}

/// <summary>
/// Request model for updating an existing article
/// </summary>
public record UpdateArticleRequest
{
    public string? Title { get; init; }
    public string? Content { get; init; }
    public Guid? CategoryId { get; init; }
    public List<string>? Tags { get; init; }
    public ArticleStatus? Status { get; init; }
}

/// <summary>
/// Request model for rating an article
/// </summary>
public record RateArticleRequest
{
    public bool IsHelpful { get; init; }
}

/// <summary>
/// Result for search operations with pagination
/// </summary>
public record SearchArticlesResult
{
    public List<ArticleListItemDto> Articles { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
