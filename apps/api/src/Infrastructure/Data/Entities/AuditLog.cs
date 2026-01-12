namespace Hickory.Api.Infrastructure.Data.Entities;

/// <summary>
/// Audit event types for tracking system activity
/// </summary>
public enum AuditAction
{
    // Entity operations
    Create = 0,
    Update = 1,
    Delete = 2,
    
    // Authentication events
    Login = 10,
    LoginFailed = 11,
    Logout = 12,
    TwoFactorEnabled = 13,
    TwoFactorDisabled = 14,
    TwoFactorVerified = 15,
    TwoFactorFailed = 16,
    PasswordChanged = 17,
    
    // User management
    UserCreated = 20,
    UserUpdated = 21,
    UserDeactivated = 22,
    UserActivated = 23,
    RoleChanged = 24,
    
    // Ticket lifecycle
    TicketCreated = 30,
    TicketUpdated = 31,
    TicketAssigned = 32,
    TicketClosed = 33,
    TicketReopened = 34,
    CommentAdded = 35
}

/// <summary>
/// Immutable audit log entry for tracking all significant system events
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit entry
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// When the event occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// The type of action performed
    /// </summary>
    public AuditAction Action { get; set; }
    
    /// <summary>
    /// User who performed the action (null for system events or failed logins)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Email of user at time of event (for historical reference)
    /// </summary>
    public string? UserEmail { get; set; }
    
    /// <summary>
    /// Type of entity affected (e.g., "Ticket", "User", "Comment")
    /// </summary>
    public string? EntityType { get; set; }
    
    /// <summary>
    /// ID of the affected entity
    /// </summary>
    public string? EntityId { get; set; }
    
    /// <summary>
    /// JSON-serialized previous values (for updates/deletes)
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// JSON-serialized new values (for creates/updates)
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// Client IP address
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Client user agent string
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Additional context or error message
    /// </summary>
    public string? Details { get; set; }
}
