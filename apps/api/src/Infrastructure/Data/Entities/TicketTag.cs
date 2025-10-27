namespace Hickory.Api.Infrastructure.Data.Entities;

/// <summary>
/// Many-to-many join table linking tickets to tags
/// </summary>
public class TicketTag
{
    /// <summary>
    /// The ticket ID
    /// </summary>
    public Guid TicketId { get; set; }

    /// <summary>
    /// The tag ID
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    /// When this tag was added to the ticket
    /// </summary>
    public DateTime AddedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// The ticket this tag is associated with
    /// </summary>
    public Ticket Ticket { get; set; } = null!;

    /// <summary>
    /// The tag associated with the ticket
    /// </summary>
    public Tag Tag { get; set; } = null!;
}
