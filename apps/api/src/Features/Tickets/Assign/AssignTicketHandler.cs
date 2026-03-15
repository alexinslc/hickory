using Hickory.Api.Common.Events;
using Hickory.Api.Infrastructure.Data;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.Assign;

public record AssignTicketCommand(Guid TicketId, Guid AgentId, Guid AssignedById) : IRequest<Unit>;

public class AssignTicketHandler : IRequestHandler<AssignTicketCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AssignTicketHandler(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
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

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "The ticket was modified by another user. Please refresh and try again.");
        }

        // Load submitter and assigner info for the event
        var submitter = await _dbContext.Users
            .Where(u => u.Id == ticket.SubmitterId)
            .Select(u => new { u.FirstName, u.LastName, u.Email })
            .FirstOrDefaultAsync(cancellationToken);

        var assignedBy = await _dbContext.Users
            .Where(u => u.Id == command.AssignedById)
            .Select(u => new { u.FirstName, u.LastName, u.Email })
            .FirstOrDefaultAsync(cancellationToken);

        await _publishEndpoint.Publish(new TicketAssignedEvent
        {
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            SubmitterId = ticket.SubmitterId,
            SubmitterName = submitter != null ? $"{submitter.FirstName} {submitter.LastName}" : "Unknown",
            SubmitterEmail = submitter?.Email ?? "",
            AssignedToId = agent.Id,
            AssignedToName = $"{agent.FirstName} {agent.LastName}",
            AssignedToEmail = agent.Email,
            AssignedById = command.AssignedById,
            AssignedByName = assignedBy != null ? $"{assignedBy.FirstName} {assignedBy.LastName}" : "Unknown",
            AssignedByEmail = assignedBy?.Email ?? "",
            AssignedAt = DateTime.UtcNow
        }, cancellationToken);

        return Unit.Value;
    }
}
