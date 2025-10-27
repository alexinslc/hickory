using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.RemoveTags;

public record RemoveTagsFromTicketCommand(
    Guid TicketId,
    List<string> TagNames
) : IRequest<Unit>;

public class RemoveTagsFromTicketHandler : IRequestHandler<RemoveTagsFromTicketCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public RemoveTagsFromTicketHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(RemoveTagsFromTicketCommand request, CancellationToken cancellationToken)
    {
        // Find tag IDs by names (case-insensitive)
        var normalizedNames = request.TagNames.Select(n => n.ToLowerInvariant()).ToList();
        var tagIds = await _dbContext.Tags
            .Where(t => normalizedNames.Contains(t.Name.ToLower()))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var ticketTags = await _dbContext.TicketTags
            .Where(tt => tt.TicketId == request.TicketId && tagIds.Contains(tt.TagId))
            .ToListAsync(cancellationToken);

        if (ticketTags.Any())
        {
            _dbContext.TicketTags.RemoveRange(ticketTags);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
