using Hickory.Api.Features.Auth.Models;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Auth.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<AuthResponse>;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<RegisterHandler> _logger;
    private readonly double _jwtExpirationMinutes;

    public RegisterHandler(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        ILogger<RegisterHandler> logger,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
        _jwtExpirationMinutes = double.Parse(configuration["JWT:ExpirationMinutes"] ?? "60");
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            throw new InvalidOperationException("Email already registered");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.EndUser, // Default role
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        
        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenString = _tokenService.GenerateRefreshToken();
        
        // Store refresh token in database
        var refreshToken = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30) // 30-day refresh token expiration
        };
        
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New user registered: {UserId}", user.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes)
        };
    }
}
