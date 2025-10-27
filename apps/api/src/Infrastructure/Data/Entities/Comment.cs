namespace Hickory.Api.Infrastructure.Data.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; } // Internal notes visible only to agents
    
    // Relationships
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
