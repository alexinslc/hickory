namespace Hickory.Api.Infrastructure.Data.Entities;

/// <summary>
/// User notification preferences for email, in-app, and webhook notifications
/// </summary>
public class NotificationPreferences
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Email preferences
    public bool EmailEnabled { get; set; } = true;
    public bool EmailOnTicketCreated { get; set; } = true;
    public bool EmailOnTicketUpdated { get; set; } = true;
    public bool EmailOnTicketAssigned { get; set; } = true;
    public bool EmailOnCommentAdded { get; set; } = true;
    
    // In-app/SignalR preferences
    public bool InAppEnabled { get; set; } = true;
    public bool InAppOnTicketCreated { get; set; } = true;
    public bool InAppOnTicketUpdated { get; set; } = true;
    public bool InAppOnTicketAssigned { get; set; } = true;
    public bool InAppOnCommentAdded { get; set; } = true;
    
    // Webhook preferences
    public bool WebhookEnabled { get; set; } = false;
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public User User { get; set; } = null!;
}
