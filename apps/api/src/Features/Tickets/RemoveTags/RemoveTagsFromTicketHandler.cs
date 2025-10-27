using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.RemoveTags;

public record RemoveTagsFromTicketCommand(
    Guid TicketId,
    List<Guid> TagIds
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
        var ticketTags = await _dbContext.TicketTags
            .Where(tt => tt.TicketId == request.TicketId && request.TagIds.Contains(tt.TagId))
            .ToListAsync(cancellationToken);

        if (ticketTags.Any())
        {
            _dbContext.TicketTags.RemoveRange(ticketTags);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
