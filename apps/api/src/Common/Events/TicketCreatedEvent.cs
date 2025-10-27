namespace Hickory.Api.Common.Events;

/// <summary>
/// Event published when a new ticket is created
/// </summary>
public record TicketCreatedEvent
{
    public required Guid TicketId { get; init; }
    public required string TicketNumber { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required string Priority { get; init; }
    public required Guid SubmitterId { get; init; }
    public required string SubmitterName { get; init; }
    public required string SubmitterEmail { get; init; }
    public Guid? AssignedToId { get; init; }
    public string? AssignedToName { get; init; }
    public string? AssignedToEmail { get; init; }
    public DateTime CreatedAt { get; init; }
}
