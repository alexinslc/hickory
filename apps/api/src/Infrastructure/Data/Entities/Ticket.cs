namespace Hickory.Api.Infrastructure.Data.Entities;

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3,
    Cancelled = 4
}

public enum TicketPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public class Ticket
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    
    // Relationships
    public Guid SubmitterId { get; set; }
    public User Submitter { get; set; } = null!;
    
    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    
    // Category
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    // Resolution
    public string? ResolutionNotes { get; set; }
    
    // Optimistic Concurrency
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
    // Full-text search vector (PostgreSQL tsvector)
    public NpgsqlTypes.NpgsqlTsVector SearchVector { get; set; } = null!;
    
    // Collections
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<TicketTag> TicketTags { get; set; } = new List<TicketTag>();
}
