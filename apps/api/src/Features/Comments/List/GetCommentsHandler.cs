using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Comments.List;

public record GetCommentsQuery(Guid TicketId, string UserRole) : IRequest<List<CommentDto>>;

public class GetCommentsHandler : IRequestHandler<GetCommentsQuery, List<CommentDto>>
{
    private readonly ApplicationDbContext _context;

    public GetCommentsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommentDto>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var isAgent = request.UserRole == "Agent" || request.UserRole == "Administrator";

        var query = _context.Comments
            .Include(c => c.Author)
            .Where(c => c.TicketId == request.TicketId);

        // Only show internal notes to agents and admins
        if (!isAgent)
        {
            query = query.Where(c => !c.IsInternal);
        }

        var comments = await query
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                IsInternal = c.IsInternal,
                AuthorName = c.Author.FirstName + " " + c.Author.LastName,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return comments;
    }
}
