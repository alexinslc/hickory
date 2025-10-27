namespace Hickory.Api.Infrastructure.Data.Entities;

/// <summary>
/// Represents a classification for organizing tickets (e.g., "Hardware", "Software", "Network Access")
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the category
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Category name (e.g., "Hardware", "Software")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the category
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for sorting categories in UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Optional hex color code for category badge (e.g., "#3B82F6")
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// When the category was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the category was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Whether the category is active and available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Tickets assigned to this category
    /// </summary>
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
