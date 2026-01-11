using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Auth.Logout;

public record LogoutCommand(Guid UserId, string? RefreshToken) : IRequest<Unit>;

/// <summary>
/// Handles user logout by revoking all active refresh tokens for the user.
/// This prevents the use of any existing refresh tokens after logout.
/// </summary>
public class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        ApplicationDbContext context,
        ILogger<LogoutHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Handles the logout command by revoking all active refresh tokens for the user.
    /// </summary>
    /// <param name="request">The logout command containing the user ID and optional refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Unit</returns>
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Revoke all active tokens for this user
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "User logout";
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User {UserId} logged out, revoked {Count} tokens", request.UserId, activeTokens.Count);

        return Unit.Value;
    }
}
