using Hickory.Api.Common.Events;
using Hickory.Api.Infrastructure.Caching;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hickory.Api.Features.Tickets.UpdateStatus;

public record UpdateTicketStatusCommand(Guid TicketId, TicketStatus NewStatus, Guid UpdatedById) : IRequest<Unit>;

public class UpdateTicketStatusHandler : IRequestHandler<UpdateTicketStatusCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UpdateTicketStatusHandler> _logger;

    public UpdateTicketStatusHandler(ApplicationDbContext dbContext, ICacheService cacheService, IPublishEndpoint publishEndpoint, ILogger<UpdateTicketStatusHandler> logger)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
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

        var oldStatus = ticket.Status;
        ticket.Status = command.NewStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Invalidate ticket cache
            await _cacheService.RemoveAsync(CacheKeys.Ticket(command.TicketId), cancellationToken);
            // Invalidate related list caches
            await _cacheService.RemoveByPatternAsync(CacheKeys.AllTicketsPattern(), cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Ticket was modified by another user - throw exception to return 409 Conflict
            throw new InvalidOperationException(
                "The ticket was modified by another user. Please refresh and try again.");
        }

        // Publish event for email notifications
        var submitter = await _dbContext.Users
            .Where(u => u.Id == ticket.SubmitterId)
            .Select(u => new { u.FirstName, u.LastName, u.Email })
            .FirstOrDefaultAsync(cancellationToken);

        var updatedBy = await _dbContext.Users
            .Where(u => u.Id == command.UpdatedById)
            .Select(u => new { u.FirstName, u.LastName, u.Email })
            .FirstOrDefaultAsync(cancellationToken);

        var assignedTo = ticket.AssignedToId.HasValue
            ? await _dbContext.Users
                .Where(u => u.Id == ticket.AssignedToId.Value)
                .Select(u => new { u.FirstName, u.LastName, u.Email })
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        try
        {
            await _publishEndpoint.Publish(new TicketUpdatedEvent
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status.ToString(),
                Priority = ticket.Priority.ToString(),
                SubmitterId = ticket.SubmitterId,
                SubmitterName = submitter != null ? $"{submitter.FirstName} {submitter.LastName}" : "Unknown",
                SubmitterEmail = submitter?.Email ?? "",
                AssignedToId = ticket.AssignedToId,
                AssignedToName = assignedTo != null ? $"{assignedTo.FirstName} {assignedTo.LastName}" : null,
                AssignedToEmail = assignedTo?.Email,
                UpdatedById = command.UpdatedById,
                UpdatedByName = updatedBy != null ? $"{updatedBy.FirstName} {updatedBy.LastName}" : "Unknown",
                UpdatedByEmail = updatedBy?.Email ?? "",
                UpdatedAt = ticket.UpdatedAt,
                ChangedFields = new List<string> { $"Status: {oldStatus} -> {command.NewStatus}" }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish TicketUpdatedEvent for ticket {TicketId}", ticket.Id);
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
