using Hickory.Api.Common.Events;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Notifications;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Notifications.Consumers;

/// <summary>
/// Consumes ticket events and sends webhook notifications to configured endpoints
/// </summary>
public class WebhookNotificationConsumer :
    IConsumer<TicketCreatedEvent>,
    IConsumer<TicketUpdatedEvent>,
    IConsumer<TicketAssignedEvent>,
    IConsumer<CommentAddedEvent>
{
    private readonly IWebhookService _webhookService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<WebhookNotificationConsumer> _logger;

    public WebhookNotificationConsumer(
        IWebhookService webhookService,
        ApplicationDbContext dbContext,
        ILogger<WebhookNotificationConsumer> logger)
    {
        _webhookService = webhookService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TicketCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing webhook notification for ticket created: {TicketNumber}", message.TicketNumber);

        var webhookUrls = await GetWebhookUrlsAsync(context.CancellationToken);
        
        foreach (var url in webhookUrls)
        {
            await _webhookService.SendTicketCreatedWebhookAsync(url, message, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<TicketUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing webhook notification for ticket updated: {TicketNumber}", message.TicketNumber);

        var webhookUrls = await GetWebhookUrlsAsync(context.CancellationToken);
        
        foreach (var url in webhookUrls)
        {
            await _webhookService.SendTicketUpdatedWebhookAsync(url, message, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<TicketAssignedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing webhook notification for ticket assigned: {TicketNumber}", message.TicketNumber);

        var webhookUrls = await GetWebhookUrlsAsync(context.CancellationToken);
        
        foreach (var url in webhookUrls)
        {
            await _webhookService.SendTicketAssignedWebhookAsync(url, message, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<CommentAddedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing webhook notification for comment added: {TicketNumber}", message.TicketNumber);

        var webhookUrls = await GetWebhookUrlsAsync(context.CancellationToken);
        
        foreach (var url in webhookUrls)
        {
            await _webhookService.SendCommentAddedWebhookAsync(url, message, context.CancellationToken);
        }
    }

    private async Task<List<string>> GetWebhookUrlsAsync(CancellationToken cancellationToken)
    {
        // Query NotificationPreferences for enabled webhook URLs
        return await _dbContext.NotificationPreferences
            .Where(p => p.WebhookEnabled && !string.IsNullOrEmpty(p.WebhookUrl))
            .Select(p => p.WebhookUrl!)
            .ToListAsync(cancellationToken);
    }
}
