using Hickory.Api.Common.Events;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hickory.Api.Features.Tickets.Reassign;

public record ReassignTicketCommand(Guid TicketId, Guid NewAgentId, Guid ReassignedById) : IRequest<Unit>;

public class ReassignTicketHandler : IRequestHandler<ReassignTicketCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ReassignTicketHandler> _logger;

    public ReassignTicketHandler(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, ILogger<ReassignTicketHandler> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
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

        // Publish event for email notifications
        var submitter = await _dbContext.Users
            .Where(u => u.Id == ticket.SubmitterId)
            .Select(u => new { u.FirstName, u.LastName, u.Email })
            .FirstOrDefaultAsync(cancellationToken);

        var reassignedBy = await _dbContext.Users
            .Where(u => u.Id == command.ReassignedById)
            .Select(u => new { u.FirstName, u.LastName, u.Email })
            .FirstOrDefaultAsync(cancellationToken);

        try
        {
            await _publishEndpoint.Publish(new TicketAssignedEvent
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                SubmitterId = ticket.SubmitterId,
                SubmitterName = submitter != null ? $"{submitter.FirstName} {submitter.LastName}" : "Unknown",
                SubmitterEmail = submitter?.Email ?? "",
                AssignedToId = newAgent.Id,
                AssignedToName = $"{newAgent.FirstName} {newAgent.LastName}",
                AssignedToEmail = newAgent.Email,
                AssignedById = command.ReassignedById,
                AssignedByName = reassignedBy != null ? $"{reassignedBy.FirstName} {reassignedBy.LastName}" : "Unknown",
                AssignedByEmail = reassignedBy?.Email ?? "",
                AssignedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish TicketAssignedEvent for ticket {TicketId}", ticket.Id);
        }

        return Unit.Value;
    }
}
