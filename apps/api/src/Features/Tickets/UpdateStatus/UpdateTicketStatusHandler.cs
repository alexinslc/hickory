using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.UpdateStatus;

public record UpdateTicketStatusCommand(Guid TicketId, TicketStatus NewStatus) : IRequest<Unit>;

public class UpdateTicketStatusHandler : IRequestHandler<UpdateTicketStatusCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateTicketStatusHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateTicketStatusCommand command, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID {command.TicketId} not found");
        }

        // Validate status transitions (will be enhanced with FluentValidation later)
        ValidateStatusTransition(ticket.Status, command.NewStatus);

        ticket.Status = command.NewStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Ticket was modified by another user - throw exception to return 409 Conflict
            throw new InvalidOperationException(
                "The ticket was modified by another user. Please refresh and try again.");
        }

        return Unit.Value;
    }

    private void ValidateStatusTransition(TicketStatus currentStatus, TicketStatus newStatus)
    {
        // Closed and Cancelled tickets cannot be reopened
        if (currentStatus == TicketStatus.Closed || currentStatus == TicketStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot change status of {currentStatus} ticket");
        }

        // Can't directly close a ticket (use CloseTicket command instead for resolution notes)
        if (newStatus == TicketStatus.Closed)
        {
            throw new InvalidOperationException("Use CloseTicket command to close a ticket with resolution notes");
        }
    }
}
