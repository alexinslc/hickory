namespace Hickory.Api.Common.Events;

/// <summary>
/// Event published when a ticket is updated
/// </summary>
public record TicketUpdatedEvent
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
    public required Guid UpdatedById { get; init; }
    public required string UpdatedByName { get; init; }
    public required string UpdatedByEmail { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<string> ChangedFields { get; init; } = new();
}
