using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Features.Tickets.UpdateStatus;

public record UpdateTicketStatusRequest
{
    public TicketStatus NewStatus { get; init; }
}
