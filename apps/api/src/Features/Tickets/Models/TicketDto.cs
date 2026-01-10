using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Features.Tickets.Models;

public record TicketDto
{
    public Guid Id { get; init; }
    public string TicketNumber { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public Guid SubmitterId { get; init; }
    public string SubmitterName { get; init; } = string.Empty;
    public Guid? AssignedToId { get; init; }
    public string? AssignedToName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public string? ResolutionNotes { get; init; }
    public int CommentCount { get; init; }
    public string? RowVersion { get; init; } // Base64 encoded for JSON, nullable for cached responses
    
    // Category and Tags for organization
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public List<string> Tags { get; init; } = new();
}

public record CommentDto
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsInternal { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record AttachmentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public Guid UploadedById { get; init; }
    public string UploadedByName { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}
