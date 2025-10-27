using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Features.Tickets.UpdatePriority;

public record UpdateTicketPriorityRequest
{
    public TicketPriority NewPriority { get; init; }
}
