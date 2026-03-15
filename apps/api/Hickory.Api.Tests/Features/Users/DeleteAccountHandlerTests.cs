using FluentAssertions;
using Hickory.Api.Features.Users.DeleteAccount;
using Hickory.Api.Infrastructure.Audit;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Hickory.Api.Tests.Features.Users;

public class DeleteAccountHandlerTests
{
    private readonly Mock<IAuditLogService> _auditLogServiceMock;

    public DeleteAccountHandlerTests()
    {
        _auditLogServiceMock = new Mock<IAuditLogService>();
    }

    [Fact]
    public async Task Handle_ValidUser_AnonymizesUserData()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "john@example.com",
            firstName: "John",
            lastName: "Doe"
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteAccountHandler(dbContext, _auditLogServiceMock.Object);
        var command = new DeleteAccountCommand(user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("deleted");

        var updatedUser = await dbContext.Users.FirstAsync(u => u.Id == user.Id);
        updatedUser.Email.Should().Contain("anonymized.local");
        updatedUser.FirstName.Should().Be("Deleted");
        updatedUser.LastName.Should().Be("User");
        updatedUser.PasswordHash.Should().BeNull();
        updatedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidUser_AnonymizesComments()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);

        var ticket = TestDataBuilder.CreateTestTicket(submitterId: user.Id);
        dbContext.Tickets.Add(ticket);

        var comment = TestDataBuilder.CreateTestComment(
            ticketId: ticket.Id,
            authorId: user.Id,
            content: "This contains personal info"
        );
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteAccountHandler(dbContext, _auditLogServiceMock.Object);
        var command = new DeleteAccountCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedComment = await dbContext.Comments.FirstAsync(c => c.Id == comment.Id);
        updatedComment.Content.Should().Contain("removed due to account deletion");
    }

    [Fact]
    public async Task Handle_ValidUser_RevokesRefreshTokens()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteAccountHandler(dbContext, _auditLogServiceMock.Object);
        var command = new DeleteAccountCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedToken = await dbContext.RefreshTokens.FirstAsync(rt => rt.Id == refreshToken.Id);
        updatedToken.RevokedAt.Should().NotBeNull();
        updatedToken.RevokedReason.Should().Be("Account deleted");
    }

    [Fact]
    public async Task Handle_ValidUser_ClearsTwoFactorData()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        user.TwoFactorEnabled = true;
        user.TwoFactorSecret = "JBSWY3DPEHPK3PXP";
        user.TwoFactorBackupCodes = "[\"code1\",\"code2\"]";
        user.TwoFactorEnabledAt = DateTime.UtcNow;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteAccountHandler(dbContext, _auditLogServiceMock.Object);
        var command = new DeleteAccountCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedUser = await dbContext.Users.FirstAsync(u => u.Id == user.Id);
        updatedUser.TwoFactorEnabled.Should().BeFalse();
        updatedUser.TwoFactorSecret.Should().BeNull();
        updatedUser.TwoFactorBackupCodes.Should().BeNull();
        updatedUser.TwoFactorEnabledAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ValidUser_LogsAuditEvent()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(email: "john@example.com");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteAccountHandler(dbContext, _auditLogServiceMock.Object);
        var command = new DeleteAccountCommand(user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _auditLogServiceMock.Verify(
            a => a.LogAsync(
                AuditAction.AccountDeleted,
                user.Id,
                "john@example.com",
                "User",
                user.Id.ToString(),
                null,
                null,
                It.Is<string>(s => s.Contains("GDPR")),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_NonExistentUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new DeleteAccountHandler(dbContext, _auditLogServiceMock.Object);
        var command = new DeleteAccountCommand(Guid.NewGuid());

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
