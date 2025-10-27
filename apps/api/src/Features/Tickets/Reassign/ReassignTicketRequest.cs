namespace Hickory.Api.Features.Tickets.Reassign;

public record ReassignTicketRequest
{
    public Guid NewAgentId { get; init; }
}
