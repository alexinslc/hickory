namespace Hickory.Api.Features.EmailIntegration.InboundWebhook;

/// <summary>
/// Normalized inbound email payload that supports common webhook formats
/// (SendGrid Inbound Parse, Mailgun Routes, Postmark, etc.)
/// </summary>
public record InboundEmailRequest
{
    /// <summary>
    /// Sender email address (SendGrid: "from", Mailgun: "sender" or "from")
    /// </summary>
    public string From { get; init; } = string.Empty;

    /// <summary>
    /// Recipient email address
    /// </summary>
    public string To { get; init; } = string.Empty;

    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Plain text body (SendGrid: "text", Mailgun: "body-plain")
    /// </summary>
    public string? TextBody { get; init; }

    /// <summary>
    /// HTML body (SendGrid: "html", Mailgun: "body-html")
    /// </summary>
    public string? HtmlBody { get; init; }

    /// <summary>
    /// Original sender display name, if available
    /// </summary>
    public string? SenderName { get; init; }

    /// <summary>
    /// Message-ID header for threading
    /// </summary>
    public string? MessageId { get; init; }

    /// <summary>
    /// In-Reply-To header for threading
    /// </summary>
    public string? InReplyTo { get; init; }

    /// <summary>
    /// References header for threading
    /// </summary>
    public string? References { get; init; }
}

public record InboundEmailResponse
{
    public Guid TicketId { get; init; }
    public string TicketNumber { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty; // "created" or "commented"
}
