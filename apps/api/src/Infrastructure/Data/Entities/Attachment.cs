namespace Hickory.Api.Infrastructure.Data.Entities;

public class Attachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty; // Path in storage system
    
    // Relationships
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    
    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;
    
    // Timestamps
    public DateTime UploadedAt { get; set; }
}
