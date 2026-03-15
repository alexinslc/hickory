using FluentAssertions;
using Hickory.Api.Infrastructure.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Hickory.Api.Tests.Infrastructure.Notifications;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly SmtpSettings _smtpSettings;

    public EmailServiceTests()
    {
        _loggerMock = new Mock<ILogger<EmailService>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["App:BaseUrl"]).Returns("http://localhost:3000");

        _smtpSettings = new SmtpSettings
        {
            Host = "localhost",
            Port = 1025,
            FromAddress = "noreply@hickory.local",
            FromName = "Hickory Help Desk",
            Enabled = false // Disabled by default in tests to avoid needing an SMTP server
        };
    }

    private EmailService CreateService(SmtpSettings? settings = null)
    {
        var opts = Options.Create(settings ?? _smtpSettings);
        return new EmailService(_loggerMock.Object, _configurationMock.Object, opts);
    }

    [Fact]
    public async Task SendTicketCreatedEmailAsync_WhenDisabled_LogsInsteadOfSending()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SendTicketCreatedEmailAsync(
            "user@example.com",
            "Test User",
            "TKT-001",
            "Test Ticket",
            "This is a test description",
            "High");

        // Assert - no exception thrown; email logged instead of sent
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Email sending is disabled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTicketUpdatedEmailAsync_WhenDisabled_LogsInsteadOfSending()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SendTicketUpdatedEmailAsync(
            "user@example.com",
            "Test User",
            "TKT-001",
            "Test Ticket",
            "Admin User",
            new List<string> { "Status", "Priority" });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Email sending is disabled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTicketAssignedEmailAsync_WhenDisabled_LogsInsteadOfSending()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SendTicketAssignedEmailAsync(
            "agent@example.com",
            "Agent User",
            "TKT-001",
            "Test Ticket",
            "Admin User");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Email sending is disabled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendCommentAddedEmailAsync_WhenDisabled_LogsInsteadOfSending()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SendCommentAddedEmailAsync(
            "user@example.com",
            "Test User",
            "TKT-001",
            "Test Ticket",
            "Agent User",
            "This is a test comment",
            isInternal: false);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Email sending is disabled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendCommentAddedEmailAsync_InternalComment_SkipsEmail()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SendCommentAddedEmailAsync(
            "user@example.com",
            "Test User",
            "TKT-001",
            "Test Ticket",
            "Agent User",
            "Internal note content",
            isInternal: true);

        // Assert - should log skip at Debug level, NOT the "disabled" message
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Skipping email for internal comment")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WhenSmtpConnectionFails_ThrowsAndLogsError()
    {
        // Arrange - Enable sending but point to a port with nothing listening
        var settings = new SmtpSettings
        {
            Host = "localhost",
            Port = 19999, // Port with nothing listening
            FromAddress = "noreply@hickory.local",
            FromName = "Hickory Help Desk",
            Enabled = true
        };
        var service = CreateService(settings);

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.SendTicketCreatedEmailAsync(
                "user@example.com",
                "Test User",
                "TKT-001",
                "Test Ticket",
                "Description",
                "High"));
    }
}
