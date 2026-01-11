using Hickory.Api.Features.Tickets.Models;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tickets.GetById;

/// <summary>
/// Get ticket with all details including comments and attachments
/// </summary>
public record GetTicketDetailsQuery(Guid TicketId, Guid UserId, string UserRole) 
    : IRequest<TicketDetailsResponse?>;

public record TicketDetailsResponse(
    TicketDto Ticket,
    List<CommentDto> Comments,
    List<AttachmentDto> Attachments
);

public class GetTicketDetailsHandler : IRequestHandler<GetTicketDetailsQuery, TicketDetailsResponse?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetTicketDetailsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TicketDetailsResponse?> Handle(
        GetTicketDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.Tickets
            .Include(t => t.Submitter)
            .Include(t => t.AssignedTo)
            .Include(t => t.Category)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.Attachments)
                .ThenInclude(a => a.UploadedBy)
            .Include(t => t.TicketTags)
                .ThenInclude(tt => tt.Tag)
            .Where(t => t.Id == query.TicketId)
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket == null)
        {
            return null;
        }

        // Authorization
        var isAgent = query.UserRole == "Agent" || query.UserRole == "Admin";
        var isOwner = ticket.SubmitterId == query.UserId;
        
        if (!isAgent && !isOwner)
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

        var comments = ticket.Comments
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                IsInternal = c.IsInternal,
                AuthorId = c.AuthorId,
                AuthorName = $"{c.Author.FirstName} {c.Author.LastName}",
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .OrderBy(c => c.CreatedAt)
            .ToList();

        var attachments = ticket.Attachments
            .Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSizeBytes = a.FileSizeBytes,
                UploadedById = a.UploadedById,
                UploadedByName = $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}",
                UploadedAt = a.UploadedAt
            })
            .OrderBy(a => a.UploadedAt)
            .ToList();

        return new TicketDetailsResponse(ticketDto, comments, attachments);
    }
}
