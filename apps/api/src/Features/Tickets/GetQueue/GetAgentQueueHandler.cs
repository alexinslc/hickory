using Hickory.Api.Common;
using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.GetQueue;

public record GetAgentQueueQuery(
    Guid? AgentId = null,
    Guid? CategoryId = null,
    List<string>? Tags = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<TicketDto>>;

public class GetAgentQueueHandler : IRequestHandler<GetAgentQueueQuery, PaginatedResult<TicketDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAgentQueueHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResult<TicketDto>> Handle(GetAgentQueueQuery query, CancellationToken cancellationToken)
    {
        // Filter: unassigned tickets OR assigned to the specified agent
        var ticketsQuery = _dbContext.Tickets
            .Include(t => t.Submitter)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .Include(t => t.Category)
            .Include(t => t.TicketTags)
                .ThenInclude(tt => tt.Tag)
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
            // No additional filtering needed
        }

        // Filter by category if provided
        if (query.CategoryId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.CategoryId == query.CategoryId.Value);
        }

        // Filter by tags if provided
        if (query.Tags != null && query.Tags.Any())
        {
            var normalizedTags = query.Tags.Select(t => t.ToLowerInvariant()).ToList();
            ticketsQuery = ticketsQuery.Where(t => 
                t.TicketTags.Any(tt => normalizedTags.Contains(tt.Tag.Name.ToLower())));
        }

        // Get total count before pagination
        var totalCount = await ticketsQuery.CountAsync(cancellationToken);

        // Apply pagination with bounds checking
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);
        var skip = (page - 1) * pageSize;

        // Sort by priority (Critical first) then by age (oldest first)
        var tickets = await ticketsQuery
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ticketDtos = tickets.Select(ticket => new TicketDto
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
            CommentCount = ticket.Comments.Count,
            RowVersion = Convert.ToBase64String(ticket.RowVersion),
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category?.Name,
            Tags = ticket.TicketTags.Select(tt => tt.Tag.Name).ToList()
        }).ToList();

        return PaginatedResult<TicketDto>.Create(ticketDtos, totalCount, page, pageSize);
    }
}
