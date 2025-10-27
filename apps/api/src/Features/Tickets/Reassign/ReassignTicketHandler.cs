using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.Reassign;

public record ReassignTicketCommand(Guid TicketId, Guid NewAgentId) : IRequest<Unit>;

public class ReassignTicketHandler : IRequestHandler<ReassignTicketCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public ReassignTicketHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(ReassignTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID {command.TicketId} not found");
        }

        // Can't reassign closed or cancelled tickets
        if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot reassign {ticket.Status} ticket");
        }

        // Verify the new agent exists and has appropriate role
        var newAgent = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == command.NewAgentId, cancellationToken);

        if (newAgent == null)
        {
            throw new KeyNotFoundException($"Agent with ID {command.NewAgentId} not found");
        }

        if (newAgent.Role != UserRole.Agent && newAgent.Role != UserRole.Administrator)
        {
            throw new InvalidOperationException("User must have Agent or Administrator role");
        }

        ticket.AssignedToId = command.NewAgentId;
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
