using System.Text.RegularExpressions;
using Hickory.Api.Common.Services;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hickory.Api.Features.EmailIntegration.InboundWebhook;

public record ProcessInboundEmailCommand(InboundEmailRequest Request) : IRequest<InboundEmailResponse>;

public partial class ProcessInboundEmailHandler : IRequestHandler<ProcessInboundEmailCommand, InboundEmailResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITicketNumberGenerator _ticketNumberGenerator;
    private readonly ILogger<ProcessInboundEmailHandler> _logger;

    // Matches patterns like [Ticket #TKT-00001] or [TKT-00001] in subject lines
    [GeneratedRegex(@"\[(?:Ticket\s*#?)?(TKT-\d{5})\]", RegexOptions.IgnoreCase)]
    private static partial Regex TicketReferencePattern();

    public ProcessInboundEmailHandler(
        ApplicationDbContext dbContext,
        ITicketNumberGenerator ticketNumberGenerator,
        ILogger<ProcessInboundEmailHandler> logger)
    {
        _dbContext = dbContext;
        _ticketNumberGenerator = ticketNumberGenerator;
        _logger = logger;
    }

    public async Task<InboundEmailResponse> Handle(ProcessInboundEmailCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var senderEmail = ParseEmailAddress(request.From);
        var body = GetEmailBody(request);

        // Try to find an existing ticket by subject reference
        var ticketNumber = ExtractTicketNumber(request.Subject);
        if (ticketNumber != null)
        {
            var existingTicket = await _dbContext.Tickets
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, cancellationToken);

            if (existingTicket != null)
            {
                return await AddCommentToTicket(existingTicket, senderEmail, body, cancellationToken);
            }

            _logger.LogWarning(
                "Ticket reference {TicketNumber} found in subject but ticket does not exist. Creating new ticket.",
                ticketNumber);
        }

        // Create a new ticket
        return await CreateTicketFromEmail(senderEmail, request.Subject, body, request.SenderName, cancellationToken);
    }

    private async Task<InboundEmailResponse> CreateTicketFromEmail(
        string senderEmail,
        string subject,
        string body,
        string? senderName,
        CancellationToken cancellationToken)
    {
        // Find or create the user by email
        var user = await FindOrCreateUser(senderEmail, senderName, cancellationToken);

        var ticketNumber = await _ticketNumberGenerator.GenerateTicketNumberAsync(cancellationToken);

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            Title = subject,
            Description = body,
            Status = TicketStatus.Open,
            Priority = TicketPriority.Medium,
            SubmitterId = user.Id
        };

        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created ticket {TicketNumber} from inbound email from {SenderEmail}",
            ticketNumber, senderEmail);

        return new InboundEmailResponse
        {
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Action = "created"
        };
    }

    private async Task<InboundEmailResponse> AddCommentToTicket(
        Ticket ticket,
        string senderEmail,
        string body,
        CancellationToken cancellationToken)
    {
        var user = await FindOrCreateUser(senderEmail, senderName: null, cancellationToken);

        // Reopen the ticket if it was closed/resolved
        if (ticket.Status is TicketStatus.Closed or TicketStatus.Resolved)
        {
            ticket.Status = TicketStatus.Open;
            _logger.LogInformation(
                "Reopened ticket {TicketNumber} due to inbound email reply from {SenderEmail}",
                ticket.TicketNumber, senderEmail);
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = body,
            IsInternal = false,
            TicketId = ticket.Id,
            AuthorId = user.Id
        };

        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Added comment to ticket {TicketNumber} from inbound email from {SenderEmail}",
            ticket.TicketNumber, senderEmail);

        return new InboundEmailResponse
        {
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Action = "commented"
        };
    }

    private async Task<User> FindOrCreateUser(string email, string? senderName, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user != null)
        {
            return user;
        }

        // Parse sender name or use email prefix as fallback
        var (firstName, lastName) = ParseSenderName(senderName, email);

        user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.EndUser,
            IsActive = true,
            PasswordHash = null // Email-created users have no password; they can set one later
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created new user {Email} from inbound email", email);

        return user;
    }

    internal static string? ExtractTicketNumber(string subject)
    {
        var match = TicketReferencePattern().Match(subject);
        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
    }

    internal static string ParseEmailAddress(string from)
    {
        // Handle formats like "John Doe <john@example.com>" or just "john@example.com"
        var angleStart = from.IndexOf('<');
        var angleEnd = from.IndexOf('>');
        if (angleStart >= 0 && angleEnd > angleStart)
        {
            return from.Substring(angleStart + 1, angleEnd - angleStart - 1).Trim().ToLowerInvariant();
        }
        return from.Trim().ToLowerInvariant();
    }

    internal static string GetEmailBody(InboundEmailRequest request)
    {
        // Prefer plain text, fall back to HTML (stripped of tags for storage)
        if (!string.IsNullOrWhiteSpace(request.TextBody))
        {
            return StripQuotedText(request.TextBody.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.HtmlBody))
        {
            var stripped = StripHtmlTags(request.HtmlBody);
            return StripQuotedText(stripped.Trim());
        }

        return string.Empty;
    }

    internal static string StripHtmlTags(string html)
    {
        // Basic HTML tag removal for email body storage
        return Regex.Replace(html, "<[^>]+>", " ")
            .Replace("&nbsp;", " ")
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Trim();
    }

    internal static string StripQuotedText(string body)
    {
        // Remove common email quoted text markers
        // Lines starting with ">" or "On ... wrote:" blocks
        var lines = body.Split('\n');
        var result = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();

            // Stop at common reply markers
            if (trimmed.StartsWith("On ") && trimmed.Contains(" wrote:"))
                break;
            if (trimmed.StartsWith(">"))
                continue;
            if (trimmed.StartsWith("-----Original Message-----"))
                break;
            if (trimmed.StartsWith("________________________________"))
                break;

            result.Add(line);
        }

        return string.Join('\n', result).TrimEnd();
    }

    private static (string FirstName, string LastName) ParseSenderName(string? senderName, string email)
    {
        if (!string.IsNullOrWhiteSpace(senderName))
        {
            var parts = senderName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? (parts[0], parts[1])
                : (parts[0], string.Empty);
        }

        // Use email prefix as first name
        var emailPrefix = email.Split('@')[0];
        return (emailPrefix, string.Empty);
    }
}
