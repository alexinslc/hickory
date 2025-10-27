namespace Hickory.Api.Infrastructure.Notifications;

/// <summary>
/// Service for sending webhook notifications
/// </summary>
public interface IWebhookService
{
    Task SendTicketCreatedWebhookAsync(
        string webhookUrl,
        object payload,
        CancellationToken cancellationToken = default);

    Task SendTicketUpdatedWebhookAsync(
        string webhookUrl,
        object payload,
        CancellationToken cancellationToken = default);

    Task SendTicketAssignedWebhookAsync(
        string webhookUrl,
        object payload,
        CancellationToken cancellationToken = default);

    Task SendCommentAddedWebhookAsync(
        string webhookUrl,
        object payload,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP POST implementation of webhook service
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(IHttpClientFactory httpClientFactory, ILogger<WebhookService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendTicketCreatedWebhookAsync(
        string webhookUrl,
        object payload,
        CancellationToken cancellationToken = default)
    {
        await SendWebhookAsync(webhookUrl, "ticket.created", payload, cancellationToken);
    }

    public async Task SendTicketUpdatedWebhookAsync(
        string webhookUrl,
        object payload,
        CancellationToken cancellationToken = default)
    {
        await SendWebhookAsync(webhookUrl, "ticket.updated", payload, cancellationToken);
    }

    public async Task SendTicketAssignedWebhookAsync(
        string webhookUrl,
        object payload,
        CancellationToken cancellationToken = default)
    {
        await SendWebhookAsync(webhookUrl, "ticket.assigned", payload, cancellationToken);
    }

    public async Task SendCommentAddedWebhookAsync(
        string webhookUrl,
        object payload,
        CancellationToken cancellationToken = default)
    {
        await SendWebhookAsync(webhookUrl, "comment.added", payload, cancellationToken);
    }

    private async Task SendWebhookAsync(
        string webhookUrl,
        string eventType,
        object payload,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("webhooks");
            var webhookPayload = new
            {
                @event = eventType,
                timestamp = DateTime.UtcNow,
                data = payload
            };

            var response = await client.PostAsJsonAsync(webhookUrl, webhookPayload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Webhook sent successfully to {WebhookUrl} for event {EventType}",
                    webhookUrl,
                    eventType);
            }
            else
            {
                _logger.LogWarning(
                    "Webhook failed with status {StatusCode} for {WebhookUrl} and event {EventType}",
                    response.StatusCode,
                    webhookUrl,
                    eventType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending webhook to {WebhookUrl} for event {EventType}",
                webhookUrl,
                eventType);
            // Don't throw - webhook failures shouldn't break the main flow
        }
    }
}
