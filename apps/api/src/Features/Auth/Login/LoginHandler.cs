using Hickory.Api.Features.Auth.Models;
using Hickory.Api.Features.Auth.TwoFactor;
using Hickory.Api.Infrastructure.Audit;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

/// <summary>
/// Result of a login attempt - can be either AuthResponse (success) or TwoFactorRequiredResponse
/// </summary>
public class LoginResult
{
    public AuthResponse? AuthResponse { get; init; }
    public TwoFactorRequiredResponse? TwoFactorRequired { get; init; }
    
    public bool RequiresTwoFactor => TwoFactorRequired != null;
    
    public static LoginResult Success(AuthResponse response) => new() { AuthResponse = response };
    public static LoginResult TwoFactorNeeded(TwoFactorRequiredResponse response) => new() { TwoFactorRequired = response };
}

public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<LoginHandler> _logger;
    private readonly double _jwtExpirationMinutes;
    private const int MaxActiveTokensPerUser = 5;

    public LoginHandler(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        IAuditLogService auditLogService,
        ILogger<LoginHandler> logger,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _auditLogService = auditLogService;
        _logger = logger;
        _jwtExpirationMinutes = double.Parse(configuration["JWT:ExpirationMinutes"] ?? "60");
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent email: {Email}", request.Email);
            await _auditLogService.LogAuthEventAsync(
                AuditAction.LoginFailed, request.Email, success: false, 
                details: "User not found", cancellationToken: cancellationToken);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
            await _auditLogService.LogAuthEventAsync(
                AuditAction.LoginFailed, user.Email, user.Id, success: false, 
                details: "Account inactive", cancellationToken: cancellationToken);
            throw new UnauthorizedAccessException("Account is inactive");
        }

        // Verify password
        if (user.PasswordHash == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password attempt for user: {UserId}", user.Id);
            await _auditLogService.LogAuthEventAsync(
                AuditAction.LoginFailed, user.Email, user.Id, success: false, 
                details: "Invalid password", cancellationToken: cancellationToken);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Check if 2FA is enabled - don't complete login yet
        if (user.TwoFactorEnabled)
        {
            _logger.LogInformation("2FA required for user {UserId}", user.Id);
            return LoginResult.TwoFactorNeeded(new TwoFactorRequiredResponse
            {
                UserId = user.Id,
                Email = user.Email
            });
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        
        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenString = _tokenService.GenerateRefreshToken();
        
        // Limit active tokens per user (max 5 devices/sessions)
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .OrderBy(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);

        // If user has reached max active tokens, revoke the oldest one
        if (activeTokens.Count >= MaxActiveTokensPerUser)
        {
            var oldestToken = activeTokens.First();
            oldestToken.RevokedAt = DateTime.UtcNow;
            oldestToken.RevokedReason = $"Exceeded maximum active sessions ({MaxActiveTokensPerUser})";
            _logger.LogInformation("Revoked oldest token for user {UserId} due to session limit", user.Id);
        }
        
        // Store refresh token in database
        var refreshToken = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30) // 30-day refresh token expiration
        };
        
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
        
        // Audit successful login
        await _auditLogService.LogAuthEventAsync(
            AuditAction.Login, user.Email, user.Id, success: true, 
            cancellationToken: cancellationToken);

        return LoginResult.Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes)
        });
    }
}
