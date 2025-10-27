using Hickory.Api.Common.Events;
using Hickory.Api.Infrastructure.RealTime;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Hickory.Api.Features.Notifications.Consumers;

/// <summary>
/// Consumes ticket events and sends real-time SignalR notifications
/// </summary>
public class SignalRNotificationConsumer :
    IConsumer<TicketCreatedEvent>,
    IConsumer<TicketUpdatedEvent>,
    IConsumer<TicketAssignedEvent>,
    IConsumer<CommentAddedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationConsumer> _logger;

    public SignalRNotificationConsumer(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TicketCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing SignalR notification for ticket created: {TicketNumber}", message.TicketNumber);

        var notification = new NotificationMessage
        {
            Type = "ticket.created",
            Title = "New Ticket Created",
            Message = $"Ticket {message.TicketNumber} has been created",
            TicketNumber = message.TicketNumber,
            TicketId = message.TicketId,
            Data = new { message.Title, message.Priority, message.Status }
        };

        // Notify the submitter
        await _hubContext.Clients
            .Group($"user-{message.SubmitterId}")
            .SendAsync("notification", notification, context.CancellationToken);

        // If assigned, notify the assignee
        if (message.AssignedToId.HasValue)
        {
            var assigneeNotification = notification with
            {
                Title = "Ticket Assigned to You",
                Message = $"Ticket {message.TicketNumber} has been assigned to you"
            };

            await _hubContext.Clients
                .Group($"user-{message.AssignedToId.Value}")
                .SendAsync("notification", assigneeNotification, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<TicketUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing SignalR notification for ticket updated: {TicketNumber}", message.TicketNumber);

        var changes = string.Join(", ", message.ChangedFields);
        var notification = new NotificationMessage
        {
            Type = "ticket.updated",
            Title = "Ticket Updated",
            Message = $"Ticket {message.TicketNumber} was updated by {message.UpdatedByName}",
            TicketNumber = message.TicketNumber,
            TicketId = message.TicketId,
            Data = new { message.Title, Changes = changes, UpdatedBy = message.UpdatedByName }
        };

        // Notify the submitter (if they didn't make the update)
        if (message.SubmitterId != message.UpdatedById)
        {
            await _hubContext.Clients
                .Group($"user-{message.SubmitterId}")
                .SendAsync("notification", notification, context.CancellationToken);
        }

        // Notify assigned agent (if they exist and didn't make the update)
        if (message.AssignedToId.HasValue && message.AssignedToId != message.UpdatedById)
        {
            await _hubContext.Clients
                .Group($"user-{message.AssignedToId.Value}")
                .SendAsync("notification", notification, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<TicketAssignedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing SignalR notification for ticket assigned: {TicketNumber}", message.TicketNumber);

        // Notify the assignee
        var assigneeNotification = new NotificationMessage
        {
            Type = "ticket.assigned",
            Title = "Ticket Assigned to You",
            Message = $"Ticket {message.TicketNumber} has been assigned to you by {message.AssignedByName}",
            TicketNumber = message.TicketNumber,
            TicketId = message.TicketId,
            Data = new { message.Title, AssignedBy = message.AssignedByName }
        };

        await _hubContext.Clients
            .Group($"user-{message.AssignedToId}")
            .SendAsync("notification", assigneeNotification, context.CancellationToken);

        // Notify the submitter
        var submitterNotification = new NotificationMessage
        {
            Type = "ticket.updated",
            Title = "Ticket Updated",
            Message = $"Ticket {message.TicketNumber} has been assigned to {message.AssignedToName}",
            TicketNumber = message.TicketNumber,
            TicketId = message.TicketId,
            Data = new { message.Title, AssignedTo = message.AssignedToName }
        };

        await _hubContext.Clients
            .Group($"user-{message.SubmitterId}")
            .SendAsync("notification", submitterNotification, context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<CommentAddedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing SignalR notification for comment added: {TicketNumber}", message.TicketNumber);

        // Don't send real-time notifications for internal comments to non-agents
        // (This would require checking user role, simplified for now)

        var notification = new NotificationMessage
        {
            Type = "comment.added",
            Title = "New Comment",
            Message = $"{message.AuthorName} commented on ticket {message.TicketNumber}",
            TicketNumber = message.TicketNumber,
            TicketId = message.TicketId,
            Data = new
            {
                TicketTitle = message.TicketTitle,
                Author = message.AuthorName,
                Content = message.CommentContent.Length > 100 
                    ? message.CommentContent.Substring(0, 100) + "..." 
                    : message.CommentContent,
                IsInternal = message.IsInternal
            }
        };

        // Notify submitter (if they didn't write the comment and it's not internal)
        if (message.SubmitterId != message.AuthorId && !message.IsInternal)
        {
            await _hubContext.Clients
                .Group($"user-{message.SubmitterId}")
                .SendAsync("notification", notification, context.CancellationToken);
        }

        // Notify assigned agent (if they exist, didn't write the comment)
        if (message.AssignedToId.HasValue && message.AssignedToId != message.AuthorId)
        {
            await _hubContext.Clients
                .Group($"user-{message.AssignedToId.Value}")
                .SendAsync("notification", notification, context.CancellationToken);
        }
    }
}
