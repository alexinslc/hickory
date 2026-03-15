using Hickory.Api.Common.Events;
using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MassTransit;
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
    private readonly IPublishEndpoint _publishEndpoint;

    public AddCommentHandler(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
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
        var authorName = author != null ? $"{author.FirstName} {author.LastName}" : "Unknown";

        // Load submitter and assigned agent info for the event
        var submitter = await _dbContext.Users
            .Where(u => u.Id == ticket.SubmitterId)
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
            .FirstOrDefaultAsync(cancellationToken);

        User? assignedTo = null;
        if (ticket.AssignedToId.HasValue)
        {
            assignedTo = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == ticket.AssignedToId.Value, cancellationToken);
        }

        await _publishEndpoint.Publish(new CommentAddedEvent
        {
            CommentId = comment.Id,
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            TicketTitle = ticket.Title,
            CommentContent = comment.Content,
            AuthorId = command.UserId,
            AuthorName = authorName,
            AuthorEmail = author?.Email ?? "",
            IsInternal = isInternal,
            SubmitterId = ticket.SubmitterId,
            SubmitterName = submitter != null ? $"{submitter.FirstName} {submitter.LastName}" : "Unknown",
            SubmitterEmail = submitter?.Email ?? "",
            AssignedToId = ticket.AssignedToId,
            AssignedToName = assignedTo != null ? $"{assignedTo.FirstName} {assignedTo.LastName}" : null,
            AssignedToEmail = assignedTo?.Email,
            CreatedAt = comment.CreatedAt
        }, cancellationToken);

        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            IsInternal = comment.IsInternal,
            AuthorId = comment.AuthorId,
            AuthorName = authorName,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
