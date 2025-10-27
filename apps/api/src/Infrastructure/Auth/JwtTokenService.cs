using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Hickory.Api.Infrastructure.Auth;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate an access token for a user
    /// </summary>
    string GenerateAccessToken(User user);
    
    /// <summary>
    /// Generate a refresh token
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validate a token and extract claims
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] 
                ?? throw new InvalidOperationException("JWT:Secret not configured")));
        
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["JWT:ExpirationMinutes"] ?? "60")),
            signingCredentials: credentials
        );
        
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("Generated access token for user {UserId}", user.Id);
        
        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] 
                ?? throw new InvalidOperationException("JWT:Secret not configured")));

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }
}
