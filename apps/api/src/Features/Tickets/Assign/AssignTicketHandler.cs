using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.Assign;

public record AssignTicketCommand(Guid TicketId, Guid AgentId) : IRequest<Unit>;

public class AssignTicketHandler : IRequestHandler<AssignTicketCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public AssignTicketHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(AssignTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID {command.TicketId} not found");
        }

        // Verify the agent exists and has appropriate role
        var agent = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == command.AgentId, cancellationToken);

        if (agent == null)
        {
            throw new KeyNotFoundException($"Agent with ID {command.AgentId} not found");
        }

        if (agent.Role != Infrastructure.Data.Entities.UserRole.Agent && 
            agent.Role != Infrastructure.Data.Entities.UserRole.Administrator)
        {
            throw new InvalidOperationException("User must have Agent or Administrator role");
        }

        ticket.AssignedToId = command.AgentId;
        ticket.UpdatedAt = DateTime.UtcNow;
        
        // If ticket is still Open, move it to InProgress
        if (ticket.Status == Infrastructure.Data.Entities.TicketStatus.Open)
        {
            ticket.Status = Infrastructure.Data.Entities.TicketStatus.InProgress;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
