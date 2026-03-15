namespace Hickory.Api.Infrastructure.Notifications;

/// <summary>
/// Configuration settings for SMTP email sending
/// </summary>
public class SmtpSettings
{
    public const string SectionName = "Smtp";

    /// <summary>
    /// SMTP server hostname. Defaults to localhost for MailHog development.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// SMTP server port. Defaults to 1025 for MailHog development.
    /// </summary>
    public int Port { get; set; } = 1025;

    /// <summary>
    /// SMTP username for authentication. Leave empty for MailHog.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// SMTP password for authentication. Leave empty for MailHog.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Sender email address.
    /// </summary>
    public string FromAddress { get; set; } = "noreply@hickory.local";

    /// <summary>
    /// Sender display name.
    /// </summary>
    public string FromName { get; set; } = "Hickory Help Desk";

    /// <summary>
    /// Secure socket option for the SMTP connection.
    /// Valid values: None, StartTls, StartTlsWhenAvailable, SslOnConnect, Auto.
    /// Defaults to None for MailHog development. Use SslOnConnect for port 465 (implicit TLS)
    /// or StartTls for port 587 (explicit TLS).
    /// </summary>
    public string SecureSocketOption { get; set; } = "None";

    /// <summary>
    /// Whether email sending is enabled. When false, emails are logged but not sent.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
