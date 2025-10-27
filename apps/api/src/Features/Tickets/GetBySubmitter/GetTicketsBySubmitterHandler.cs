using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.GetBySubmitter;

public record GetTicketsBySubmitterQuery(
    Guid UserId, 
    Guid? CategoryId = null,
    List<string>? Tags = null
) : IRequest<List<TicketDto>>;

public class GetTicketsBySubmitterHandler : IRequestHandler<GetTicketsBySubmitterQuery, List<TicketDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetTicketsBySubmitterHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TicketDto>> Handle(GetTicketsBySubmitterQuery query, CancellationToken cancellationToken)
    {
        var ticketsQuery = _dbContext.Tickets
            .Include(t => t.Submitter)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .Include(t => t.Category)
            .Include(t => t.TicketTags)
                .ThenInclude(tt => tt.Tag)
            .Where(t => t.SubmitterId == query.UserId);

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

        var tickets = await ticketsQuery
            .OrderByDescending(t => t.CreatedAt)
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
            CommentCount = ticket.Comments.Count,
            RowVersion = Convert.ToBase64String(ticket.RowVersion),
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category?.Name,
            Tags = ticket.TicketTags.Select(tt => tt.Tag.Name).ToList()
        }).ToList();
    }
}
