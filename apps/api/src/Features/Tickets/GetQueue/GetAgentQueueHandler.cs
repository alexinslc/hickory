using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.GetQueue;

public record GetAgentQueueQuery(Guid? AgentId = null) : IRequest<List<TicketDto>>;

public class GetAgentQueueHandler : IRequestHandler<GetAgentQueueQuery, List<TicketDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAgentQueueHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TicketDto>> Handle(GetAgentQueueQuery query, CancellationToken cancellationToken)
    {
        // Filter: unassigned tickets OR assigned to the specified agent
        var ticketsQuery = _dbContext.Tickets
            .Include(t => t.Submitter)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .Where(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Cancelled);

        if (query.AgentId.HasValue)
        {
            // Show unassigned tickets + tickets assigned to this agent
            ticketsQuery = ticketsQuery.Where(t => 
                t.AssignedToId == null || t.AssignedToId == query.AgentId.Value);
        }
        else
        {
            // Show all non-closed tickets for queue overview
            ticketsQuery = ticketsQuery.Where(t => true);
        }

        // Sort by priority (Critical first) then by age (oldest first)
        var tickets = await ticketsQuery
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tickets.Select(ticket => new TicketDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            SubmitterId = ticket.SubmitterId,
            SubmitterName = $"{ticket.Submitter.FirstName} {ticket.Submitter.LastName}",
            AssignedToId = ticket.AssignedToId,
            AssignedToName = ticket.AssignedTo != null 
                ? $"{ticket.AssignedTo.FirstName} {ticket.AssignedTo.LastName}"
                : null,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ClosedAt = ticket.ClosedAt,
            ResolutionNotes = ticket.ResolutionNotes,
            CommentCount = ticket.Comments.Count
        }).ToList();
    }
}
