using Hickory.Api.Common;
using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.GetBySubmitter;

public record GetTicketsBySubmitterQuery(
    Guid UserId, 
    Guid? CategoryId = null,
    List<string>? Tags = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<TicketDto>>;

public class GetTicketsBySubmitterHandler : IRequestHandler<GetTicketsBySubmitterQuery, PaginatedResult<TicketDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetTicketsBySubmitterHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResult<TicketDto>> Handle(GetTicketsBySubmitterQuery query, CancellationToken cancellationToken)
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

        // Get total count before pagination
        var totalCount = await ticketsQuery.CountAsync(cancellationToken);

        // Apply pagination with bounds checking
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);
        var skip = (page - 1) * pageSize;

        // Use projection to avoid loading unnecessary data and prevent N+1 queries
        var tickets = await ticketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
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

        return PaginatedResult<TicketDto>.Create(tickets, totalCount, page, pageSize);
    }
}
