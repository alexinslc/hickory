using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Users.DataExport;

public record ExportUserDataQuery(Guid UserId) : IRequest<UserDataExportDto>;

public record UserDataExportDto
{
    public UserProfileExportDto Profile { get; init; } = null!;
    public List<TicketExportDto> Tickets { get; init; } = new();
    public List<CommentExportDto> Comments { get; init; } = new();
    public DateTime ExportedAt { get; init; }
}

public record UserProfileExportDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

public record TicketExportDto
{
    public Guid Id { get; init; }
    public string TicketNumber { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public string? ResolutionNotes { get; init; }
}

public record CommentExportDto
{
    public Guid Id { get; init; }
    public Guid TicketId { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsInternal { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public class ExportUserDataHandler : IRequestHandler<ExportUserDataQuery, UserDataExportDto>
{
    private readonly ApplicationDbContext _dbContext;

    public ExportUserDataHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDataExportDto> Handle(ExportUserDataQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {request.UserId} not found");

        var tickets = await _dbContext.Tickets
            .AsNoTracking()
            .Where(t => t.SubmitterId == request.UserId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketExportDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt,
                ResolutionNotes = t.ResolutionNotes
            })
            .ToListAsync(cancellationToken);

        var comments = await _dbContext.Comments
            .AsNoTracking()
            .Where(c => c.AuthorId == request.UserId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentExportDto
            {
                Id = c.Id,
                TicketId = c.TicketId,
                Content = c.Content,
                IsInternal = c.IsInternal,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new UserDataExportDto
        {
            Profile = new UserProfileExportDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            },
            Tickets = tickets,
            Comments = comments,
            ExportedAt = DateTime.UtcNow
        };
    }
}
