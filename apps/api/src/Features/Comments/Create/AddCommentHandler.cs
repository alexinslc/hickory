using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Comments.Create;

public record AddCommentRequest
{
    public string Content { get; init; } = string.Empty;
    public bool IsInternal { get; init; }
}

public record AddCommentCommand(Guid TicketId, AddCommentRequest Request, Guid UserId, string UserRole) : IRequest<CommentDto>;

public class AddCommentHandler : IRequestHandler<AddCommentCommand, CommentDto>
{
    private readonly ApplicationDbContext _dbContext;

    public AddCommentHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommentDto> Handle(AddCommentCommand command, CancellationToken cancellationToken)
    {
        // Verify ticket exists
        var ticket = await _dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new InvalidOperationException("Ticket not found");
        }

        // Only agents can create internal notes
        var isAgent = command.UserRole == "Agent" || command.UserRole == "Admin";
        var isInternal = command.Request.IsInternal && isAgent;

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = command.Request.Content,
            IsInternal = isInternal,
            TicketId = command.TicketId,
            AuthorId = command.UserId
        };

        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Load author for response
        var author = await _dbContext.Users.FindAsync(new object[] { command.UserId }, cancellationToken);

        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            IsInternal = comment.IsInternal,
            AuthorId = comment.AuthorId,
            AuthorName = author != null ? $"{author.FirstName} {author.LastName}" : "Unknown",
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
