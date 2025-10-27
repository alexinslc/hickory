using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.Close;

public record CloseTicketCommand(Guid TicketId, string ResolutionNotes) : IRequest<Unit>;

public class CloseTicketHandler : IRequestHandler<CloseTicketCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public CloseTicketHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(CloseTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID {command.TicketId} not found");
        }

        // Can't close an already closed or cancelled ticket
        if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Cancelled)
        {
            throw new InvalidOperationException($"Ticket is already {ticket.Status}");
        }

        // Resolution notes are required
        if (string.IsNullOrWhiteSpace(command.ResolutionNotes))
        {
            throw new ArgumentException("Resolution notes are required when closing a ticket");
        }

        ticket.Status = TicketStatus.Closed;
        ticket.ResolutionNotes = command.ResolutionNotes;
        ticket.ClosedAt = DateTime.UtcNow;
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
