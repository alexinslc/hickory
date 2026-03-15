using FluentAssertions;
using Hickory.Api.Common.Services;
using Hickory.Api.Features.EmailIntegration.InboundWebhook;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hickory.Api.Tests.Features.EmailIntegration;

public class ProcessInboundEmailHandlerTests
{
    private readonly Mock<ITicketNumberGenerator> _ticketNumberGeneratorMock;
    private readonly Mock<ILogger<ProcessInboundEmailHandler>> _loggerMock;

    public ProcessInboundEmailHandlerTests()
    {
        _ticketNumberGeneratorMock = new Mock<ITicketNumberGenerator>();
        _loggerMock = new Mock<ILogger<ProcessInboundEmailHandler>>();

        _ticketNumberGeneratorMock
            .Setup(g => g.GenerateTicketNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("TKT-00001");
    }

    [Fact]
    public async Task Handle_NewEmail_CreatesTicket()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = CreateHandler(dbContext);

        var request = new InboundEmailRequest
        {
            From = "customer@example.com",
            To = "support@company.com",
            Subject = "Help with my account",
            TextBody = "I need help resetting my password."
        };

        var command = new ProcessInboundEmailCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Action.Should().Be("created");
        result.TicketNumber.Should().Be("TKT-00001");

        var ticket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.TicketNumber == "TKT-00001");
        ticket.Should().NotBeNull();
        ticket!.Title.Should().Be("Help with my account");
        ticket.Description.Should().Be("I need help resetting my password.");
        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.Priority.Should().Be(TicketPriority.Medium);
    }

    [Fact]
    public async Task Handle_NewEmail_CreatesUserIfNotFound()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = CreateHandler(dbContext);

        var request = new InboundEmailRequest
        {
            From = "newuser@example.com",
            To = "support@company.com",
            Subject = "New issue",
            TextBody = "I have a problem.",
            SenderName = "Jane Smith"
        };

        var command = new ProcessInboundEmailCommand(request);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        user.Should().NotBeNull();
        user!.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.Role.Should().Be(UserRole.EndUser);
    }

    [Fact]
    public async Task Handle_NewEmail_UsesExistingUser()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var existingUser = TestDataBuilder.CreateTestUser(email: "existing@example.com", firstName: "Existing", lastName: "User");
        dbContext.Users.Add(existingUser);
        await dbContext.SaveChangesAsync();

        var handler = CreateHandler(dbContext);

        var request = new InboundEmailRequest
        {
            From = "existing@example.com",
            To = "support@company.com",
            Subject = "Another issue",
            TextBody = "I have another problem."
        };

        var command = new ProcessInboundEmailCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var ticket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == result.TicketId);
        ticket!.SubmitterId.Should().Be(existingUser.Id);

        // Should not create a duplicate user
        var userCount = await dbContext.Users.CountAsync(u => u.Email == "existing@example.com");
        userCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ReplyWithTicketReference_AddsComment()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(email: "customer@example.com");
        var ticket = TestDataBuilder.CreateTestTicket(submitterId: user.Id, ticketNumber: "TKT-00042");
        dbContext.Users.Add(user);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = CreateHandler(dbContext);

        var request = new InboundEmailRequest
        {
            From = "customer@example.com",
            To = "support@company.com",
            Subject = "Re: Help with my account [TKT-00042]",
            TextBody = "Thanks, but I still need help."
        };

        var command = new ProcessInboundEmailCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Action.Should().Be("commented");
        result.TicketNumber.Should().Be("TKT-00042");

        var comments = await dbContext.Comments.Where(c => c.TicketId == ticket.Id).ToListAsync();
        comments.Should().HaveCount(1);
        comments[0].Content.Should().Be("Thanks, but I still need help.");
        comments[0].IsInternal.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReplyToClosedTicket_ReopensTicket()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(email: "customer@example.com");
        var ticket = TestDataBuilder.CreateTestTicket(
            submitterId: user.Id,
            ticketNumber: "TKT-00042",
            status: TicketStatus.Closed);
        dbContext.Users.Add(user);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = CreateHandler(dbContext);

        var request = new InboundEmailRequest
        {
            From = "customer@example.com",
            To = "support@company.com",
            Subject = "Re: [Ticket #TKT-00042] Issue",
            TextBody = "Actually, this is not resolved."
        };

        var command = new ProcessInboundEmailCommand(request);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FirstAsync(t => t.Id == ticket.Id);
        updatedTicket.Status.Should().Be(TicketStatus.Open);
    }

    [Fact]
    public async Task Handle_EmailWithHtmlBody_StripsHtml()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = CreateHandler(dbContext);

        var request = new InboundEmailRequest
        {
            From = "customer@example.com",
            To = "support@company.com",
            Subject = "HTML email",
            HtmlBody = "<p>Hello, I need <strong>help</strong> with my account.</p>"
        };

        var command = new ProcessInboundEmailCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var ticket = await dbContext.Tickets.FirstAsync(t => t.Id == result.TicketId);
        ticket.Description.Should().NotContain("<p>");
        ticket.Description.Should().NotContain("<strong>");
        ticket.Description.Should().Contain("help");
    }

    [Fact]
    public async Task Handle_EmailWithQuotedText_StripsQuotes()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(email: "customer@example.com");
        var ticket = TestDataBuilder.CreateTestTicket(submitterId: user.Id, ticketNumber: "TKT-00042");
        dbContext.Users.Add(user);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = CreateHandler(dbContext);

        var request = new InboundEmailRequest
        {
            From = "customer@example.com",
            To = "support@company.com",
            Subject = "Re: [TKT-00042] Issue",
            TextBody = "This is my reply.\n\nOn Monday, Agent wrote:\n> Previous message content"
        };

        var command = new ProcessInboundEmailCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var comment = await dbContext.Comments.FirstAsync(c => c.TicketId == ticket.Id);
        comment.Content.Should().Be("This is my reply.");
    }

    [Fact]
    public async Task Handle_EmailFromDisplayNameFormat_ParsesEmailCorrectly()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = CreateHandler(dbContext);

        var request = new InboundEmailRequest
        {
            From = "John Doe <john@example.com>",
            To = "support@company.com",
            Subject = "Test",
            TextBody = "Test body content here"
        };

        var command = new ProcessInboundEmailCommand(request);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "john@example.com");
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NonexistentTicketReference_CreatesNewTicket()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = CreateHandler(dbContext);

        var request = new InboundEmailRequest
        {
            From = "customer@example.com",
            To = "support@company.com",
            Subject = "Re: [TKT-99999] Old issue",
            TextBody = "Following up on this."
        };

        var command = new ProcessInboundEmailCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Action.Should().Be("created");
        result.TicketNumber.Should().Be("TKT-00001");
    }

    [Theory]
    [InlineData("[TKT-00042]", "TKT-00042")]
    [InlineData("[Ticket #TKT-00042]", "TKT-00042")]
    [InlineData("Re: [TKT-00042] Help needed", "TKT-00042")]
    [InlineData("Re: Help needed [Ticket #TKT-00001]", "TKT-00001")]
    [InlineData("No ticket reference here", null)]
    [InlineData("Almost [TKT-123] but wrong format", null)]
    public void ExtractTicketNumber_VariousSubjects_ReturnsExpected(string subject, string? expected)
    {
        var result = ProcessInboundEmailHandler.ExtractTicketNumber(subject);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("john@example.com", "john@example.com")]
    [InlineData("John Doe <john@example.com>", "john@example.com")]
    [InlineData("  JOHN@EXAMPLE.COM  ", "john@example.com")]
    public void ParseEmailAddress_VariousFormats_ReturnsNormalizedEmail(string input, string expected)
    {
        var result = ProcessInboundEmailHandler.ParseEmailAddress(input);
        result.Should().Be(expected);
    }

    private ProcessInboundEmailHandler CreateHandler(
        ApplicationDbContext dbContext)
    {
        return new ProcessInboundEmailHandler(
            dbContext,
            _ticketNumberGeneratorMock.Object,
            _loggerMock.Object);
    }
}
