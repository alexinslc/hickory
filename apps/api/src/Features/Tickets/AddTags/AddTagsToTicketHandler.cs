using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.AddTags;

public record AddTagsToTicketCommand(
    Guid TicketId,
    List<string> TagNames
) : IRequest<Unit>;

public class AddTagsToTicketHandler : IRequestHandler<AddTagsToTicketCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public AddTagsToTicketHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(AddTagsToTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .Include(t => t.TicketTags)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new InvalidOperationException($"Ticket with ID {request.TicketId} not found");
        }

        foreach (var tagName in request.TagNames)
        {
            // Find or create tag
            var normalizedName = tagName.ToLowerInvariant();
            var tag = await _dbContext.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedName, cancellationToken);

            if (tag == null)
            {
                // Auto-create tag on first use
                tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = tagName
                };
                _dbContext.Tags.Add(tag);
                // SaveChangesAsync will be called once after the loop
            }

            // Add tag to ticket if not already added
            if (!ticket.TicketTags.Any(tt => tt.TagId == tag.Id))
            {
                ticket.TicketTags.Add(new TicketTag
                {
                    TicketId = ticket.Id,
                    TagId = tag.Id
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
