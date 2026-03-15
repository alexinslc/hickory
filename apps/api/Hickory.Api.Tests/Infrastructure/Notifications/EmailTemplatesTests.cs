using FluentAssertions;
using Hickory.Api.Infrastructure.Notifications;

namespace Hickory.Api.Tests.Infrastructure.Notifications;

public class EmailTemplatesTests
{
    [Fact]
    public void TicketCreated_ReturnsHtmlWithTicketDetails()
    {
        // Act
        var html = EmailTemplates.TicketCreated(
            "John Doe",
            "TKT-001",
            "Login Issue",
            "Cannot login to the system",
            "High",
            "http://localhost:3000/tickets/TKT-001");

        // Assert
        html.Should().Contain("Hickory Help Desk");
        html.Should().Contain("New Ticket Created");
        html.Should().Contain("John Doe");
        html.Should().Contain("TKT-001");
        html.Should().Contain("Login Issue");
        html.Should().Contain("Cannot login to the system");
        html.Should().Contain("High");
        html.Should().Contain("http://localhost:3000/tickets/TKT-001");
        html.Should().Contain("View Ticket");
    }

    [Fact]
    public void TicketCreated_SanitizesHtmlInInput()
    {
        // Act
        var html = EmailTemplates.TicketCreated(
            "<script>alert('xss')</script>",
            "TKT-001",
            "Test <b>bold</b>",
            "Description with <img src=x>",
            "High",
            "http://localhost:3000/tickets/TKT-001");

        // Assert - should be HTML-encoded
        html.Should().NotContain("<script>");
        html.Should().NotContain("<img src=x>");
        html.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void TicketUpdated_ReturnsHtmlWithChangedFields()
    {
        // Act
        var html = EmailTemplates.TicketUpdated(
            "Jane Doe",
            "TKT-002",
            "Network Issue",
            "Admin User",
            new List<string> { "Status", "Priority" },
            "http://localhost:3000/tickets/TKT-002");

        // Assert
        html.Should().Contain("Ticket Updated");
        html.Should().Contain("Jane Doe");
        html.Should().Contain("TKT-002");
        html.Should().Contain("Admin User");
        html.Should().Contain("Status");
        html.Should().Contain("Priority");
    }

    [Fact]
    public void TicketAssigned_ReturnsHtmlWithAssignment()
    {
        // Act
        var html = EmailTemplates.TicketAssigned(
            "Agent Smith",
            "TKT-003",
            "Printer Issue",
            "Manager Jones",
            "http://localhost:3000/tickets/TKT-003");

        // Assert
        html.Should().Contain("Ticket Assigned to You");
        html.Should().Contain("Agent Smith");
        html.Should().Contain("TKT-003");
        html.Should().Contain("Manager Jones");
    }

    [Fact]
    public void CommentAdded_ReturnsHtmlWithComment()
    {
        // Act
        var html = EmailTemplates.CommentAdded(
            "John Doe",
            "TKT-004",
            "Email Issue",
            "Support Agent",
            "We are looking into this issue and will get back to you shortly.",
            "http://localhost:3000/tickets/TKT-004");

        // Assert
        html.Should().Contain("New Comment on Ticket");
        html.Should().Contain("John Doe");
        html.Should().Contain("TKT-004");
        html.Should().Contain("Email Issue");
        html.Should().Contain("Support Agent");
        html.Should().Contain("We are looking into this issue");
    }

    [Theory]
    [InlineData("critical", "#dc2626")]
    [InlineData("high", "#ea580c")]
    [InlineData("medium", "#ca8a04")]
    [InlineData("low", "#16a34a")]
    [InlineData("unknown", "#6b7280")]
    public void TicketCreated_UsesPriorityColors(string priority, string expectedColor)
    {
        // Act
        var html = EmailTemplates.TicketCreated(
            "User",
            "TKT-001",
            "Title",
            "Description",
            priority,
            "http://localhost:3000/tickets/TKT-001");

        // Assert
        html.Should().Contain(expectedColor);
    }
}
