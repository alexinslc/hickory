using Hickory.Api.Infrastructure.Audit;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Users.DeleteAccount;

public record DeleteAccountCommand(Guid UserId) : IRequest<DeleteAccountResponse>;

public record DeleteAccountResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime DeletedAt { get; init; }
}

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand, DeleteAccountResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAuditLogService _auditLogService;

    public DeleteAccountHandler(ApplicationDbContext dbContext, IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
    }

    public async Task<DeleteAccountResponse> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {request.UserId} not found");

        var userEmail = user.Email;

        // Anonymize user data
        user.Email = $"deleted-{user.Id}@anonymized.local";
        user.FirstName = "Deleted";
        user.LastName = "User";
        user.PasswordHash = null;
        user.IsActive = false;
        user.ExternalProviderId = null;
        user.ExternalProvider = null;
        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.TwoFactorBackupCodes = null;
        user.TwoFactorEnabledAt = null;

        // Anonymize user's comments
        var comments = await _dbContext.Comments
            .Where(c => c.AuthorId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var comment in comments)
        {
            comment.Content = "[Content removed due to account deletion]";
        }

        // Revoke any active refresh tokens
        var refreshTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Account deleted";
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Log the deletion for audit compliance
        await _auditLogService.LogAsync(
            AuditAction.UserDeactivated,
            userId: request.UserId,
            userEmail: userEmail,
            entityType: "User",
            entityId: request.UserId.ToString(),
            details: "Account deleted and data anonymized per user request (GDPR right to erasure)",
            cancellationToken: cancellationToken);

        return new DeleteAccountResponse
        {
            Success = true,
            Message = "Your account has been deleted and your personal data has been anonymized.",
            DeletedAt = DateTime.UtcNow
        };
    }
}
