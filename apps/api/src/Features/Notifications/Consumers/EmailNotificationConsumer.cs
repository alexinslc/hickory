using Hickory.Api.Common.Events;
using Hickory.Api.Infrastructure.Notifications;
using MassTransit;

namespace Hickory.Api.Features.Notifications.Consumers;

/// <summary>
/// Consumes ticket events and sends email notifications
/// </summary>
public class EmailNotificationConsumer :
    IConsumer<TicketCreatedEvent>,
    IConsumer<TicketUpdatedEvent>,
    IConsumer<TicketAssignedEvent>,
    IConsumer<CommentAddedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationConsumer> _logger;

    public EmailNotificationConsumer(IEmailService emailService, ILogger<EmailNotificationConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TicketCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing email notification for ticket created: {TicketNumber}", message.TicketNumber);

        // Send email to ticket submitter
        await _emailService.SendTicketCreatedEmailAsync(
            message.SubmitterEmail,
            message.SubmitterName,
            message.TicketNumber,
            message.Title,
            message.Description,
            message.Priority,
            context.CancellationToken);

        // If assigned to someone, notify them too
        if (message.AssignedToId.HasValue && !string.IsNullOrEmpty(message.AssignedToEmail))
        {
            await _emailService.SendTicketAssignedEmailAsync(
                message.AssignedToEmail,
                message.AssignedToName ?? "Agent",
                message.TicketNumber,
                message.Title,
                message.SubmitterName,
                context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<TicketUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing email notification for ticket updated: {TicketNumber}", message.TicketNumber);

        // Send email to ticket submitter
        await _emailService.SendTicketUpdatedEmailAsync(
            message.SubmitterEmail,
            message.SubmitterName,
            message.TicketNumber,
            message.Title,
            message.UpdatedByName,
            message.ChangedFields,
            context.CancellationToken);

        // If assigned to someone and they didn't make the update, notify them too
        if (message.AssignedToId.HasValue && 
            !string.IsNullOrEmpty(message.AssignedToEmail) &&
            message.AssignedToId != message.UpdatedById)
        {
            await _emailService.SendTicketUpdatedEmailAsync(
                message.AssignedToEmail,
                message.AssignedToName ?? "Agent",
                message.TicketNumber,
                message.Title,
                message.UpdatedByName,
                message.ChangedFields,
                context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<TicketAssignedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing email notification for ticket assigned: {TicketNumber}", message.TicketNumber);

        // Notify the assigned agent
        await _emailService.SendTicketAssignedEmailAsync(
            message.AssignedToEmail,
            message.AssignedToName,
            message.TicketNumber,
            message.Title,
            message.AssignedByName,
            context.CancellationToken);

        // Notify the submitter
        await _emailService.SendTicketUpdatedEmailAsync(
            message.SubmitterEmail,
            message.SubmitterName,
            message.TicketNumber,
            message.Title,
            message.AssignedByName,
            new List<string> { "Assigned To" },
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<CommentAddedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing email notification for comment added: {TicketNumber}", message.TicketNumber);

        // Don't notify the comment author
        // Notify submitter if they didn't write the comment
        if (message.SubmitterId != message.AuthorId)
        {
            await _emailService.SendCommentAddedEmailAsync(
                message.SubmitterEmail,
                message.SubmitterName,
                message.TicketNumber,
                message.TicketTitle,
                message.AuthorName,
                message.CommentContent,
                message.IsInternal,
                context.CancellationToken);
        }

        // Notify assigned agent if they exist, didn't write the comment, and it's not internal or they're an agent
        if (message.AssignedToId.HasValue &&
            !string.IsNullOrEmpty(message.AssignedToEmail) &&
            message.AssignedToId != message.AuthorId)
        {
            await _emailService.SendCommentAddedEmailAsync(
                message.AssignedToEmail,
                message.AssignedToName ?? "Agent",
                message.TicketNumber,
                message.TicketTitle,
                message.AuthorName,
                message.CommentContent,
                message.IsInternal,
                context.CancellationToken);
        }
    }
}
