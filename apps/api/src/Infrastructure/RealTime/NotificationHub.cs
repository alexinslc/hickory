using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Hickory.Api.Infrastructure.RealTime;

/// <summary>
/// SignalR hub for real-time notifications
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromClaims();
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            _logger.LogInformation("User {UserId} connected to NotificationHub with connection {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserIdFromClaims();
        
        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private string? GetUserIdFromClaims()
    {
        return Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    }
}

/// <summary>
/// Notification message sent to clients
/// </summary>
public record NotificationMessage
{
    public required string Type { get; init; } // "ticket.created", "ticket.updated", etc.
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required string TicketNumber { get; init; }
    public Guid? TicketId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public object? Data { get; init; }
}
