namespace Hickory.Api.Infrastructure.Notifications;

/// <summary>
/// Simple HTML email templates for ticket notifications
/// </summary>
public static class EmailTemplates
{
    private static string WrapInLayout(string title, string bodyContent, string ticketUrl)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{title}</title>
            </head>
            <body style="margin:0; padding:0; background-color:#f4f5f7; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f5f7; padding:24px 0;">
                    <tr>
                        <td align="center">
                            <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1);">
                                <!-- Header -->
                                <tr>
                                    <td style="background-color:#1a56db; padding:20px 32px;">
                                        <h1 style="margin:0; color:#ffffff; font-size:20px; font-weight:600;">Hickory Help Desk</h1>
                                    </td>
                                </tr>
                                <!-- Body -->
                                <tr>
                                    <td style="padding:32px;">
                                        {bodyContent}
                                        <div style="margin-top:24px;">
                                            <a href="{ticketUrl}" style="display:inline-block; background-color:#1a56db; color:#ffffff; text-decoration:none; padding:10px 24px; border-radius:6px; font-size:14px; font-weight:500;">View Ticket</a>
                                        </div>
                                    </td>
                                </tr>
                                <!-- Footer -->
                                <tr>
                                    <td style="padding:16px 32px; background-color:#f9fafb; border-top:1px solid #e5e7eb;">
                                        <p style="margin:0; color:#6b7280; font-size:12px;">This is an automated notification from Hickory Help Desk. Please do not reply directly to this email.</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }

    public static string TicketCreated(string toName, string ticketNumber, string title, string description, string priority, string ticketUrl)
    {
        var body = $"""
            <h2 style="margin:0 0 16px; color:#111827; font-size:18px;">New Ticket Created</h2>
            <p style="margin:0 0 16px; color:#374151; font-size:14px;">Hello {Sanitize(toName)},</p>
            <p style="margin:0 0 16px; color:#374151; font-size:14px;">A new support ticket has been created:</p>
            <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%; margin-bottom:16px;">
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280; width:100px; border-radius:4px 0 0 0;">Ticket</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827; border-radius:0 4px 0 0;"><strong>{Sanitize(ticketNumber)}</strong></td>
                </tr>
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280;">Title</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827;">{Sanitize(title)}</td>
                </tr>
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280;">Priority</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827;"><span style="display:inline-block; padding:2px 8px; border-radius:4px; background-color:{PriorityColor(priority)}; color:#ffffff; font-size:12px;">{Sanitize(priority)}</span></td>
                </tr>
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280; border-radius:0 0 0 4px;">Description</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827; border-radius:0 0 4px 0;">{Sanitize(description)}</td>
                </tr>
            </table>
            """;

        return WrapInLayout($"New Ticket: {ticketNumber}", body, ticketUrl);
    }

    public static string TicketUpdated(string toName, string ticketNumber, string title, string updatedBy, List<string> changedFields, string ticketUrl)
    {
        var changes = string.Join(", ", changedFields.Select(Sanitize));
        var body = $"""
            <h2 style="margin:0 0 16px; color:#111827; font-size:18px;">Ticket Updated</h2>
            <p style="margin:0 0 16px; color:#374151; font-size:14px;">Hello {Sanitize(toName)},</p>
            <p style="margin:0 0 16px; color:#374151; font-size:14px;">Your support ticket has been updated by <strong>{Sanitize(updatedBy)}</strong>:</p>
            <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%; margin-bottom:16px;">
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280; width:100px; border-radius:4px 0 0 0;">Ticket</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827; border-radius:0 4px 0 0;"><strong>{Sanitize(ticketNumber)}</strong></td>
                </tr>
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280;">Title</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827;">{Sanitize(title)}</td>
                </tr>
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280; border-radius:0 0 0 4px;">Changes</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827; border-radius:0 0 4px 0;">{changes}</td>
                </tr>
            </table>
            """;

        return WrapInLayout($"Ticket Updated: {ticketNumber}", body, ticketUrl);
    }

    public static string TicketAssigned(string toName, string ticketNumber, string title, string assignedBy, string ticketUrl)
    {
        var body = $"""
            <h2 style="margin:0 0 16px; color:#111827; font-size:18px;">Ticket Assigned to You</h2>
            <p style="margin:0 0 16px; color:#374151; font-size:14px;">Hello {Sanitize(toName)},</p>
            <p style="margin:0 0 16px; color:#374151; font-size:14px;">A support ticket has been assigned to you by <strong>{Sanitize(assignedBy)}</strong>:</p>
            <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%; margin-bottom:16px;">
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280; width:100px; border-radius:4px 0 0 4px;">Ticket</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827; border-radius:0 4px 4px 0;"><strong>{Sanitize(ticketNumber)}</strong></td>
                </tr>
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280; border-radius:0 0 0 4px;">Title</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827; border-radius:0 0 4px 0;">{Sanitize(title)}</td>
                </tr>
            </table>
            """;

        return WrapInLayout($"Ticket Assigned: {ticketNumber}", body, ticketUrl);
    }

    public static string CommentAdded(string toName, string ticketNumber, string ticketTitle, string commentAuthor, string commentContent, string ticketUrl)
    {
        var body = $"""
            <h2 style="margin:0 0 16px; color:#111827; font-size:18px;">New Comment on Ticket</h2>
            <p style="margin:0 0 16px; color:#374151; font-size:14px;">Hello {Sanitize(toName)},</p>
            <p style="margin:0 0 16px; color:#374151; font-size:14px;"><strong>{Sanitize(commentAuthor)}</strong> has added a comment to your support ticket:</p>
            <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%; margin-bottom:16px;">
                <tr>
                    <td style="padding:8px 12px; background-color:#f3f4f6; font-size:13px; color:#6b7280; width:100px; border-radius:4px 0 0 0;">Ticket</td>
                    <td style="padding:8px 12px; background-color:#f9fafb; font-size:13px; color:#111827; border-radius:0 4px 0 0;"><strong>{Sanitize(ticketNumber)}</strong> - {Sanitize(ticketTitle)}</td>
                </tr>
            </table>
            <div style="padding:16px; background-color:#f9fafb; border-left:4px solid #1a56db; border-radius:0 4px 4px 0; margin-bottom:16px;">
                <p style="margin:0; color:#374151; font-size:14px; white-space:pre-wrap;">{Sanitize(commentContent)}</p>
            </div>
            """;

        return WrapInLayout($"Comment on Ticket: {ticketNumber}", body, ticketUrl);
    }

    private static string Sanitize(string input)
    {
        return System.Net.WebUtility.HtmlEncode(input);
    }

    private static string PriorityColor(string priority)
    {
        return priority.ToLowerInvariant() switch
        {
            "critical" => "#dc2626",
            "high" => "#ea580c",
            "medium" => "#ca8a04",
            "low" => "#16a34a",
            _ => "#6b7280"
        };
    }
}
