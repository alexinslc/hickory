namespace Hickory.Api.Common.Events;

/// <summary>
/// Event published when a comment is added to a ticket
/// </summary>
public record CommentAddedEvent
{
    public required Guid CommentId { get; init; }
    public required Guid TicketId { get; init; }
    public required string TicketNumber { get; init; }
    public required string TicketTitle { get; init; }
    public required string CommentContent { get; init; }
    public required Guid AuthorId { get; init; }
    public required string AuthorName { get; init; }
    public required string AuthorEmail { get; init; }
    public required bool IsInternal { get; init; }
    public required Guid SubmitterId { get; init; }
    public required string SubmitterName { get; init; }
    public required string SubmitterEmail { get; init; }
    public Guid? AssignedToId { get; init; }
    public string? AssignedToName { get; init; }
    public string? AssignedToEmail { get; init; }
    public DateTime CreatedAt { get; init; }
}
