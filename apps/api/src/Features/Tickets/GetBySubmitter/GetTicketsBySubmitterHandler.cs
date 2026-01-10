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

        // Use projection to avoid loading unnecessary data and prevent N+1 queries
        var tickets = await ticketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                SubmitterId = t.SubmitterId,
                SubmitterName = $"{t.Submitter.FirstName} {t.Submitter.LastName}",
                AssignedToId = t.AssignedToId,
                AssignedToName = t.AssignedTo != null 
                    ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}"
                    : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt,
                ResolutionNotes = t.ResolutionNotes,
                CommentCount = t.Comments.Count,
                RowVersion = Convert.ToBase64String(t.RowVersion),
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : null,
                Tags = t.TicketTags.Select(tt => tt.Tag.Name).ToList()
            })
            .ToListAsync(cancellationToken);

        return tickets;
    }
}
