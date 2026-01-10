using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Caching;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.GetById;

public record GetTicketByIdQuery(Guid TicketId, Guid UserId, string UserRole) : IRequest<TicketDto?>;

public class GetTicketByIdHandler : IRequestHandler<GetTicketByIdQuery, TicketDto?>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;

    public GetTicketByIdHandler(ApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<TicketDto?> Handle(GetTicketByIdQuery query, CancellationToken cancellationToken)
    {
        // Try to get from cache first
        var cacheKey = CacheKeys.Ticket(query.TicketId);
        var cachedTicket = await _cacheService.GetAsync<TicketDto>(cacheKey, cancellationToken);
        
        if (cachedTicket != null)
        {
            // Still need to check authorization even with cached data
            var isAgent = query.UserRole == "Agent" || query.UserRole == "Admin";
            var isOwner = cachedTicket.SubmitterId == query.UserId;
            
            if (!isAgent && !isOwner)
            {
                return null;
            }
            
            return cachedTicket;
        }

        // Cache miss - fetch from database
        var ticket = await _dbContext.Tickets
            .Include(t => t.Submitter)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .Include(t => t.Category)
            .Include(t => t.TicketTags)
                .ThenInclude(tt => tt.Tag)
            .Where(t => t.Id == query.TicketId)
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket == null)
        {
            return null;
        }

        // Authorization: Users can only view their own tickets unless they're an agent/admin
        var isAgentAuth = query.UserRole == "Agent" || query.UserRole == "Admin";
        var isOwnerAuth = ticket.SubmitterId == query.UserId;
        
        if (!isAgentAuth && !isOwnerAuth)
        {
            return null;
        }

        var ticketDto = new TicketDto
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
        };
        
        // Cache the result
        await _cacheService.SetAsync(cacheKey, ticketDto, CacheExpiration.TicketDetails, cancellationToken);

        return ticketDto;
    }
}
