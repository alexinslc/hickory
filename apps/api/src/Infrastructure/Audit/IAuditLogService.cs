using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Infrastructure.Audit;

/// <summary>
/// Service for capturing audit events
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Log an audit event asynchronously
    /// </summary>
    Task LogAsync(
        AuditAction action,
        Guid? userId = null,
        string? userEmail = null,
        string? entityType = null,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? details = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Log an authentication event (login, logout, failed attempt)
    /// </summary>
    Task LogAuthEventAsync(
        AuditAction action,
        string email,
        Guid? userId = null,
        bool success = true,
        string? details = null,
        CancellationToken cancellationToken = default);
}
