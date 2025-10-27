using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Features.Tickets.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Search;

public record SearchTicketsQuery(
    string? SearchQuery,
    Guid UserId,
    string UserRole,
    TicketStatus? Status,
    TicketPriority? Priority,
    Guid? AssignedToId,
    DateTime? CreatedAfter,
    DateTime? CreatedBefore,
    int Page = 1,
    int PageSize = 20
) : IRequest<SearchTicketsResult>;

public record SearchTicketsResult(
    List<TicketDto> Tickets,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public class SearchTicketsHandler : IRequestHandler<SearchTicketsQuery, SearchTicketsResult>
{
    private readonly ApplicationDbContext _dbContext;

    public SearchTicketsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchTicketsResult> Handle(SearchTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Tickets
            .Include(t => t.Submitter)
            .Include(t => t.AssignedTo)
            .Include(t => t.Category)
            .Include(t => t.TicketTags)
                .ThenInclude(tt => tt.Tag)
            .AsQueryable();

        // Parse search term once for reuse
        var searchTerm = !string.IsNullOrWhiteSpace(request.SearchQuery)
            ? EF.Functions.ToTsQuery("english", string.Join(" & ", request.SearchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries)))
            : null;

        // Authorization: Users can only search their own tickets unless they're an agent/admin
        var isAgent = request.UserRole == "Agent" || request.UserRole == "Admin";
        if (!isAgent)
        {
            query = query.Where(t => t.SubmitterId == request.UserId);
        }

        // Full-text search using PostgreSQL tsvector
        if (searchTerm != null)
        {
            query = query.Where(t => t.SearchVector.Matches(searchTerm));
        }

        // Filter by status
        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        // Filter by priority
        if (request.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == request.Priority.Value);
        }

        // Filter by assignee
        if (request.AssignedToId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == request.AssignedToId.Value);
        }

        // Filter by date range
        if (request.CreatedAfter.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.CreatedAfter.Value);
        }

        if (request.CreatedBefore.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.CreatedBefore.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Order by relevance (rank) if searching, otherwise by created date
        if (searchTerm != null)
        {
            query = query.OrderByDescending(t => t.SearchVector.Rank(searchTerm));
        }
        else
        {
            query = query.OrderByDescending(t => t.CreatedAt);
        }

        // Pagination
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Max(request.Page, 1);
        var skip = (page - 1) * pageSize;

        var tickets = await query
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

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new SearchTicketsResult(
            tickets,
            totalCount,
            page,
            pageSize,
            totalPages
        );
    }
}
