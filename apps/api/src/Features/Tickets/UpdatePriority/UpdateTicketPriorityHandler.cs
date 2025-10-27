using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.UpdatePriority;

public record UpdateTicketPriorityCommand(Guid TicketId, TicketPriority NewPriority) : IRequest<Unit>;

public class UpdateTicketPriorityHandler : IRequestHandler<UpdateTicketPriorityCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateTicketPriorityHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateTicketPriorityCommand command, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID {command.TicketId} not found");
        }

        // Can't change priority of closed tickets
        if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot change priority of {ticket.Status} ticket");
        }

        ticket.Priority = command.NewPriority;
        ticket.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "The ticket was modified by another user. Please refresh and try again.");
        }

        return Unit.Value;
    }
}
