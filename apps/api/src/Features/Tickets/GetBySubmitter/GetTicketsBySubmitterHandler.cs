using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.GetBySubmitter;

public record GetTicketsBySubmitterQuery(Guid UserId) : IRequest<List<TicketDto>>;

public class GetTicketsBySubmitterHandler : IRequestHandler<GetTicketsBySubmitterQuery, List<TicketDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetTicketsBySubmitterHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TicketDto>> Handle(GetTicketsBySubmitterQuery query, CancellationToken cancellationToken)
    {
        var tickets = await _dbContext.Tickets
            .Include(t => t.Submitter)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .Where(t => t.SubmitterId == query.UserId)
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
            CommentCount = ticket.Comments.Count
        }).ToList();
    }
}
