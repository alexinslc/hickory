using Hickory.Api.Features.Auth.Models;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

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

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the refresh token in the database
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token not found: {Token}", request.RefreshToken);
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
