using Hickory.Api.Features.Auth.Models;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

/// <summary>
/// Handles refresh token rotation and validation.
/// Implements automatic token rotation where each refresh invalidates the old token and issues a new one.
/// Includes reuse detection to prevent security breaches - if a revoked token is used, all user tokens are revoked.
/// </summary>
public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<RefreshTokenHandler> _logger;
    private readonly double _jwtExpirationMinutes;

    public RefreshTokenHandler(
        ApplicationDbContext context,
        IJwtTokenService tokenService,
        ILogger<RefreshTokenHandler> logger,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
        _jwtExpirationMinutes = double.Parse(configuration["JWT:ExpirationMinutes"] ?? "60");
    }

    /// <summary>
    /// Handles the refresh token command by validating the token, rotating it, and issuing new tokens.
    /// </summary>
    /// <param name="request">The refresh token command containing the refresh token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new AuthResponse with rotated access and refresh tokens</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when token is invalid, expired, revoked, or user is inactive</exception>
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the refresh token in the database
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            var token = request.RefreshToken ?? string.Empty;
            var visibleChars = Math.Min(8, token.Length);
            var tokenPreview = visibleChars > 0
                ? token[..visibleChars] + new string('*', Math.Max(0, token.Length - visibleChars))
                : string.Empty;

            _logger.LogWarning(
                "Refresh token not found. Token preview: {TokenPreview}, Length: {TokenLength}",
                tokenPreview,
                token.Length);
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if token is active (not expired and not revoked)
        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Inactive refresh token used for user {UserId}", refreshToken.UserId);
            
            // Token reuse detection - revoke all tokens for this user
            if (refreshToken.IsRevoked)
            {
                _logger.LogWarning("Token reuse detected for user {UserId}. Revoking all tokens.", refreshToken.UserId);
                await RevokeAllUserTokensAsync(refreshToken.UserId, "Token reuse detected", cancellationToken);
            }
            
            throw new UnauthorizedAccessException("Refresh token is no longer valid");
        }

        var user = refreshToken.User 
            ?? throw new InvalidOperationException("User not found for refresh token");

        if (!user.IsActive)
        {
            _logger.LogWarning("Refresh attempt for inactive user: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Account is inactive");
        }

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenString = _tokenService.GenerateRefreshToken();

        // Create new refresh token
        var newRefreshToken = new Infrastructure.Data.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30) // 30-day expiration
        };

        // Revoke the old refresh token (rotation)
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByToken = newRefreshTokenString;
        refreshToken.RevokedReason = "Replaced by new token";

        // Add new token and save
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token rotated for user {UserId}", user.Id);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes)
        };
    }

    /// <summary>
    /// Revokes all active refresh tokens for a specific user.
    /// Used for security purposes when token reuse is detected.
    /// </summary>
    /// <param name="userId">The user ID whose tokens should be revoked</param>
    /// <param name="reason">The reason for revocation (for audit logging)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken cancellationToken)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Revoked {Count} tokens for user {UserId}", activeTokens.Count, userId);
    }
}
