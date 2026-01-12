using System.Text.Json;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Infrastructure.Audit;

/// <summary>
/// Service for capturing and persisting audit events
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditLogService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        AuditAction action,
        Guid? userId = null,
        string? userEmail = null,
        string? entityType = null,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Action = action,
                UserId = userId,
                UserEmail = userEmail,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, JsonOptions) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues, JsonOptions) : null,
                IpAddress = GetClientIpAddress(httpContext),
                UserAgent = GetUserAgent(httpContext),
                Details = details
            };
            
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug(
                "Audit: {Action} by {UserId} on {EntityType}/{EntityId}",
                action, userId, entityType, entityId);
        }
        catch (Exception ex)
        {
            // Don't let audit logging failures break the application
            _logger.LogError(ex, "Failed to write audit log for action {Action}", action);
        }
    }

    public async Task LogAuthEventAsync(
        AuditAction action,
        string email,
        Guid? userId = null,
        bool success = true,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            action,
            userId: userId,
            userEmail: email,
            entityType: "Auth",
            entityId: userId?.ToString(),
            details: details ?? (success ? null : "Failed"),
            cancellationToken: cancellationToken);
    }

    private static string? GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null) return null;
        
        // Check X-Forwarded-For header first (for reverse proxy scenarios)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the chain (original client)
            return forwardedFor.Split(',')[0].Trim();
        }
        
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetUserAgent(HttpContext? httpContext)
    {
        var userAgent = httpContext?.Request.Headers.UserAgent.FirstOrDefault();
        
        // Truncate if too long
        if (userAgent?.Length > 500)
        {
            return userAgent[..500];
        }
        
        return userAgent;
    }
}
