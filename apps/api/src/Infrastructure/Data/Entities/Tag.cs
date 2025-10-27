namespace Hickory.Api.Infrastructure.Data.Entities;

/// <summary>
/// Represents a flexible label for organizing and filtering tickets
/// </summary>
public class Tag
{
    /// <summary>
    /// Unique identifier for the tag
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tag name (e.g., "urgent", "bug", "feature-request")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional hex color code for tag badge (e.g., "#EF4444")
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// When the tag was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Join table entries linking this tag to tickets
    /// </summary>
    public ICollection<TicketTag> TicketTags { get; set; } = new List<TicketTag>();
}
