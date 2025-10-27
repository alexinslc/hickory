namespace Hickory.Api.Common.Events;

/// <summary>
/// Event published when a ticket is assigned to an agent
/// </summary>
public record TicketAssignedEvent
{
    public required Guid TicketId { get; init; }
    public required string TicketNumber { get; init; }
    public required string Title { get; init; }
    public required Guid SubmitterId { get; init; }
    public required string SubmitterName { get; init; }
    public required string SubmitterEmail { get; init; }
    public required Guid AssignedToId { get; init; }
    public required string AssignedToName { get; init; }
    public required string AssignedToEmail { get; init; }
    public required Guid AssignedById { get; init; }
    public required string AssignedByName { get; init; }
    public required string AssignedByEmail { get; init; }
    public DateTime AssignedAt { get; init; }
}
