using Hickory.Api.Features.Auth.Models;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Hickory.Api.Features.Auth.TwoFactor;

[ApiController]
[Route("api/v1/auth/2fa")]
[EnableRateLimiting("auth")]
public class TwoFactorController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<TwoFactorController> _logger;
    private readonly IConfiguration _configuration;
    private const int MaxActiveTokensPerUser = 5;

    public TwoFactorController(
        ApplicationDbContext context,
        ITwoFactorService twoFactorService,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        ILogger<TwoFactorController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _twoFactorService = twoFactorService;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Get 2FA status for the current user
    /// </summary>
    [Authorize]
    [HttpGet("status")]
    [ProducesResponseType(typeof(TwoFactorStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TwoFactorStatusResponse>> GetStatus()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

        var backupCodesRemaining = 0;
        if (!string.IsNullOrEmpty(user.TwoFactorBackupCodes))
        {
            try
            {
                var codes = JsonSerializer.Deserialize<List<string>>(user.TwoFactorBackupCodes);
                backupCodesRemaining = codes?.Count ?? 0;
            }
            catch { }
        }

        return Ok(new TwoFactorStatusResponse
        {
            Enabled = user.TwoFactorEnabled,
            EnabledAt = user.TwoFactorEnabledAt,
            BackupCodesRemaining = backupCodesRemaining
        });
    }

    /// <summary>
    /// Initiate 2FA setup - generates secret and QR code
    /// </summary>
    [Authorize]
    [HttpPost("setup")]
    [ProducesResponseType(typeof(TwoFactorSetupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TwoFactorSetupResponse>> Setup()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

        if (user.TwoFactorEnabled)
            return BadRequest(new { message = "2FA is already enabled. Disable it first to reconfigure." });

        // Generate new secret
        var secret = _twoFactorService.GenerateSecretKey();
        var qrCodeUri = _twoFactorService.GenerateQrCodeUri(user.Email, secret);

        // Store the secret temporarily (not yet enabled)
        user.TwoFactorSecret = secret;
        await _context.SaveChangesAsync();

        _logger.LogInformation("2FA setup initiated for user {UserId}", userId);

        return Ok(new TwoFactorSetupResponse
        {
            Secret = secret,
            QrCodeUri = qrCodeUri
        });
    }

    /// <summary>
    /// Enable 2FA after verifying the setup code
    /// </summary>
    [Authorize]
    [HttpPost("enable")]
    [ProducesResponseType(typeof(TwoFactorEnableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TwoFactorEnableResponse>> Enable([FromBody] TwoFactorEnableRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

        if (user.TwoFactorEnabled)
            return BadRequest(new { message = "2FA is already enabled" });

        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            return BadRequest(new { message = "Please initiate 2FA setup first" });

        // Verify the code
        if (!_twoFactorService.ValidateCode(user.TwoFactorSecret, request.Code))
            return BadRequest(new { message = "Invalid verification code" });

        // Generate backup codes
        var backupCodes = _twoFactorService.GenerateBackupCodes();
        var hashedCodes = TwoFactorService.HashBackupCodes(backupCodes);

        // Enable 2FA
        user.TwoFactorEnabled = true;
        user.TwoFactorBackupCodes = hashedCodes;
        user.TwoFactorEnabledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("2FA enabled for user {UserId}", userId);

        return Ok(new TwoFactorEnableResponse
        {
            Enabled = true,
            BackupCodes = backupCodes
        });
    }

    /// <summary>
    /// Disable 2FA (requires password confirmation)
    /// </summary>
    [Authorize]
    [HttpPost("disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Disable([FromBody] TwoFactorDisableRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

        if (!user.TwoFactorEnabled)
            return BadRequest(new { message = "2FA is not enabled" });

        // Verify password
        if (user.PasswordHash == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return BadRequest(new { message = "Invalid password" });

        // Disable 2FA
        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.TwoFactorBackupCodes = null;
        user.TwoFactorEnabledAt = null;
        await _context.SaveChangesAsync();

        _logger.LogInformation("2FA disabled for user {UserId}", userId);

        return Ok(new { message = "2FA has been disabled" });
    }

    /// <summary>
    /// Verify 2FA code during login (completes the login process)
    /// </summary>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Verify([FromBody] TwoFactorVerifyRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
            return Unauthorized(new { message = "Invalid user" });

        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            return BadRequest(new { message = "2FA is not enabled for this user" });

        // Determine if it's a backup code (contains dash and is 9 chars)
        var isBackupCode = request.IsBackupCode ?? (request.Code.Contains('-') && request.Code.Length == 9);

        bool isValid;
        if (isBackupCode)
        {
            if (string.IsNullOrEmpty(user.TwoFactorBackupCodes))
            {
                return BadRequest(new { message = "No backup codes available" });
            }

            isValid = _twoFactorService.ValidateBackupCode(
                request.Code,
                user.TwoFactorBackupCodes,
                out var updatedCodes);

            if (isValid)
            {
                user.TwoFactorBackupCodes = updatedCodes;
                _logger.LogWarning("User {UserId} used a backup code to login", user.Id);
            }
        }
        else
        {
            isValid = _twoFactorService.ValidateCode(user.TwoFactorSecret, request.Code);
        }

        if (!isValid)
            return Unauthorized(new { message = "Invalid verification code" });

        // Complete login - generate tokens
        user.LastLoginAt = DateTime.UtcNow;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenString = _tokenService.GenerateRefreshToken();

        // Limit active tokens per user
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .OrderBy(rt => rt.CreatedAt)
            .ToListAsync();

        if (activeTokens.Count >= MaxActiveTokensPerUser)
        {
            var oldestToken = activeTokens.First();
            oldestToken.RevokedAt = DateTime.UtcNow;
            oldestToken.RevokedReason = $"Exceeded maximum active sessions ({MaxActiveTokensPerUser})";
        }

        var refreshToken = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} completed 2FA verification and logged in", user.Id);

        var jwtExpirationMinutes = double.Parse(_configuration["JWT:ExpirationMinutes"] ?? "60");

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtExpirationMinutes)
        });
    }

    /// <summary>
    /// Regenerate backup codes (requires authentication)
    /// </summary>
    [Authorize]
    [HttpPost("backup-codes/regenerate")]
    [ProducesResponseType(typeof(TwoFactorRegenerateCodesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TwoFactorRegenerateCodesResponse>> RegenerateBackupCodes()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

        if (!user.TwoFactorEnabled)
            return BadRequest(new { message = "2FA is not enabled" });

        // Generate new backup codes
        var backupCodes = _twoFactorService.GenerateBackupCodes();
        var hashedCodes = TwoFactorService.HashBackupCodes(backupCodes);

        user.TwoFactorBackupCodes = hashedCodes;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Backup codes regenerated for user {UserId}", userId);

        return Ok(new TwoFactorRegenerateCodesResponse
        {
            BackupCodes = backupCodes
        });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}
