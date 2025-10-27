namespace Hickory.Api.Infrastructure.Notifications;

/// <summary>
/// Service for sending email notifications
/// </summary>
public interface IEmailService
{
    Task SendTicketCreatedEmailAsync(
        string toEmail,
        string toName,
        string ticketNumber,
        string title,
        string description,
        string priority,
        CancellationToken cancellationToken = default);

    Task SendTicketUpdatedEmailAsync(
        string toEmail,
        string toName,
        string ticketNumber,
        string title,
        string updatedBy,
        List<string> changedFields,
        CancellationToken cancellationToken = default);

    Task SendTicketAssignedEmailAsync(
        string toEmail,
        string toName,
        string ticketNumber,
        string title,
        string assignedBy,
        CancellationToken cancellationToken = default);

    Task SendCommentAddedEmailAsync(
        string toEmail,
        string toName,
        string ticketNumber,
        string ticketTitle,
        string commentAuthor,
        string commentContent,
        bool isInternal,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// SMTP implementation of email service
/// For development, logs emails to console. In production, configure SMTP settings.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendTicketCreatedEmailAsync(
        string toEmail,
        string toName,
        string ticketNumber,
        string title,
        string description,
        string priority,
        CancellationToken cancellationToken = default)
    {
        var subject = $"New Ticket Created: {ticketNumber} - {title}";
        var body = $@"
Hello {toName},

A new support ticket has been created:

Ticket: {ticketNumber}
Title: {title}
Priority: {priority}
Description: {description}

View ticket: {GetTicketUrl(ticketNumber)}

Thank you,
Hickory Support Team
";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendTicketUpdatedEmailAsync(
        string toEmail,
        string toName,
        string ticketNumber,
        string title,
        string updatedBy,
        List<string> changedFields,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Ticket Updated: {ticketNumber} - {title}";
        var changes = string.Join(", ", changedFields);
        var body = $@"
Hello {toName},

Your support ticket has been updated by {updatedBy}:

Ticket: {ticketNumber}
Title: {title}
Changed: {changes}

View ticket: {GetTicketUrl(ticketNumber)}

Thank you,
Hickory Support Team
";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendTicketAssignedEmailAsync(
        string toEmail,
        string toName,
        string ticketNumber,
        string title,
        string assignedBy,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Ticket Assigned to You: {ticketNumber} - {title}";
        var body = $@"
Hello {toName},

A support ticket has been assigned to you by {assignedBy}:

Ticket: {ticketNumber}
Title: {title}

View ticket: {GetTicketUrl(ticketNumber)}

Thank you,
Hickory Support Team
";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendCommentAddedEmailAsync(
        string toEmail,
        string toName,
        string ticketNumber,
        string ticketTitle,
        string commentAuthor,
        string commentContent,
        bool isInternal,
        CancellationToken cancellationToken = default)
    {
        // Don't send emails about internal comments to non-agents
        if (isInternal)
        {
            _logger.LogDebug("Skipping email for internal comment on ticket {TicketNumber}", ticketNumber);
            return;
        }

        var subject = $"New Comment on Ticket: {ticketNumber} - {ticketTitle}";
        var body = $@"
Hello {toName},

{commentAuthor} has added a comment to your support ticket:

Ticket: {ticketNumber}
Title: {ticketTitle}

Comment:
{commentContent}

View ticket: {GetTicketUrl(ticketNumber)}

Thank you,
Hickory Support Team
";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        // For development, just log the email
        // In production, implement actual SMTP sending using System.Net.Mail or a service like SendGrid
        _logger.LogInformation(
            "Sending email to {Email}: {Subject}\n{Body}",
            toEmail,
            subject,
            body);

        // TODO: Implement actual email sending
        // var smtpHost = _configuration["Email:SmtpHost"];
        // var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        // var smtpUsername = _configuration["Email:Username"];
        // var smtpPassword = _configuration["Email:Password"];
        // var fromEmail = _configuration["Email:FromAddress"];
        // var fromName = _configuration["Email:FromName"];
        //
        // using var client = new SmtpClient(smtpHost, smtpPort);
        // await client.SendMailAsync(fromEmail, toEmail, subject, body);

        await Task.CompletedTask;
    }

    private string GetTicketUrl(string ticketNumber)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:3000";
        return $"{baseUrl}/tickets/{ticketNumber}";
    }
}
