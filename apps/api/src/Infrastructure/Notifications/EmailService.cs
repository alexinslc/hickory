using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

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
/// SMTP implementation of email service using MailKit.
/// Defaults to MailHog settings for development (localhost:1025, no auth).
/// Configure via Smtp section in appsettings.json or environment variables (SMTP__Host, etc.).
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SmtpSettings _smtpSettings;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration, IOptions<SmtpSettings> smtpSettings)
    {
        _logger = logger;
        _configuration = configuration;
        _smtpSettings = smtpSettings.Value;
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
        var ticketUrl = GetTicketUrl(ticketNumber);
        var htmlBody = EmailTemplates.TicketCreated(toName, ticketNumber, title, description, priority, ticketUrl);

        await SendEmailAsync(toEmail, toName, subject, htmlBody, cancellationToken);
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
        var ticketUrl = GetTicketUrl(ticketNumber);
        var htmlBody = EmailTemplates.TicketUpdated(toName, ticketNumber, title, updatedBy, changedFields, ticketUrl);

        await SendEmailAsync(toEmail, toName, subject, htmlBody, cancellationToken);
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
        var ticketUrl = GetTicketUrl(ticketNumber);
        var htmlBody = EmailTemplates.TicketAssigned(toName, ticketNumber, title, assignedBy, ticketUrl);

        await SendEmailAsync(toEmail, toName, subject, htmlBody, cancellationToken);
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
        var ticketUrl = GetTicketUrl(ticketNumber);
        var htmlBody = EmailTemplates.CommentAdded(toName, ticketNumber, ticketTitle, commentAuthor, commentContent, ticketUrl);

        await SendEmailAsync(toEmail, toName, subject, htmlBody, cancellationToken);
    }

    internal async Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        if (!_smtpSettings.Enabled)
        {
            _logger.LogInformation("Email sending is disabled. Would have sent to {Email}: {Subject}", toEmail, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();

            var secureSocketOptions = _smtpSettings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, secureSocketOptions, cancellationToken);

            // Only authenticate if credentials are provided
            if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}. SMTP Host: {Host}:{Port}",
                toEmail, subject, _smtpSettings.Host, _smtpSettings.Port);
            throw;
        }
    }

    private string GetTicketUrl(string ticketNumber)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:3000";
        return $"{baseUrl}/tickets/{ticketNumber}";
    }
}
