namespace Hickory.Api.Infrastructure.Data.Entities;

/// <summary>
/// Represents a knowledge base article for self-service support
/// </summary>
public class KnowledgeArticle
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Article title (2-200 characters)
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Article content in Markdown format
    /// </summary>
    public required string Content { get; set; }
    
    /// <summary>
    /// Optional category for organization (e.g., "Getting Started", "Troubleshooting")
    /// </summary>
    public Guid? CategoryId { get; set; }
    
    /// <summary>
    /// Category navigation property
    /// </summary>
    public Category? Category { get; set; }
    
    /// <summary>
    /// Tags for additional organization and search
    /// </summary>
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    
    /// <summary>
    /// Article status (Draft, Published, Archived)
    /// </summary>
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    
    /// <summary>
    /// View count for analytics
    /// </summary>
    public int ViewCount { get; set; }
    
    /// <summary>
    /// Number of helpful votes
    /// </summary>
    public int HelpfulCount { get; set; }
    
    /// <summary>
    /// Number of not helpful votes
    /// </summary>
    public int NotHelpfulCount { get; set; }
    
    /// <summary>
    /// Author of the article (agent or admin)
    /// </summary>
    public Guid AuthorId { get; set; }
    
    /// <summary>
    /// Author navigation property
    /// </summary>
    public User Author { get; set; } = null!;
    
    /// <summary>
    /// User who last updated the article
    /// </summary>
    public Guid? LastUpdatedById { get; set; }
    
    /// <summary>
    /// Last updater navigation property
    /// </summary>
    public User? LastUpdatedBy { get; set; }
    
    /// <summary>
    /// PostgreSQL full-text search vector (automatically computed from Title and Content)
    /// Weighted: Title='A' (highest), Content='B'
    /// </summary>
    public NpgsqlTypes.NpgsqlTsVector SearchVector { get; set; } = null!;
    
    /// <summary>
    /// Article creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Optional publish date (null if draft)
    /// </summary>
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// Article status enumeration
/// </summary>
public enum ArticleStatus
{
    /// <summary>
    /// Article is being drafted, not visible to users
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Article is published and visible to users
    /// </summary>
    Published = 1,
    
    /// <summary>
    /// Article is archived, no longer visible but kept for reference
    /// </summary>
    Archived = 2
}
