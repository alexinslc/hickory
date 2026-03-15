using Hickory.Api.Features.EmailIntegration.InboundWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hickory.Api.Features.EmailIntegration;

[ApiController]
[Route("api/webhooks")]
public class EmailWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailWebhookController> _logger;

    public EmailWebhookController(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<EmailWebhookController> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Receives inbound email webhook payloads from email providers (SendGrid, Mailgun, etc.)
    /// and creates or updates tickets accordingly.
    /// </summary>
    [HttpPost("inbound-email")]
    public async Task<ActionResult<InboundEmailResponse>> ProcessInboundEmail(
        [FromBody] InboundEmailRequest request,
        [FromHeader(Name = "X-Webhook-Secret")] string? webhookSecret,
        CancellationToken cancellationToken)
    {
        if (!ValidateWebhookSecret(webhookSecret))
        {
            _logger.LogWarning("Inbound email webhook rejected: invalid or missing webhook secret");
            return Unauthorized(new { error = "Invalid webhook secret" });
        }

        var command = new ProcessInboundEmailCommand(request);
        var response = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation(
            "Inbound email processed: ticket {TicketNumber} {Action}",
            response.TicketNumber, response.Action);

        return Ok(response);
    }

    private bool ValidateWebhookSecret(string? providedSecret)
    {
        var configuredSecret = _configuration["EmailIntegration:WebhookSecret"];

        // If no secret is configured, reject all requests (fail-closed)
        if (string.IsNullOrEmpty(configuredSecret))
        {
            _logger.LogError(
                "EmailIntegration:WebhookSecret is not configured. " +
                "All inbound email webhooks will be rejected until a secret is set.");
            return false;
        }

        return string.Equals(configuredSecret, providedSecret, StringComparison.Ordinal);
    }
}
