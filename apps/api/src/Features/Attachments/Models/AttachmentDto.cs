namespace Hickory.Api.Features.Attachments.Models;

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public Guid UploadedById { get; set; }
    public string? UploadedByName { get; set; }
}
