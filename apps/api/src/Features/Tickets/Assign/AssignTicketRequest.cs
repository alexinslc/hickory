namespace Hickory.Api.Features.Tickets.Assign;

public record AssignTicketRequest
{
    public Guid AgentId { get; init; }
}
