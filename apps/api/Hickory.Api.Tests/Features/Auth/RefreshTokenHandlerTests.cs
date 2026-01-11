using FluentAssertions;
using Hickory.Api.Features.Auth.RefreshToken;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hickory.Api.Tests.Features.Auth;

public class RefreshTokenHandlerTests
{
    private readonly Mock<IJwtTokenService> _tokenServiceMock;
    private readonly Mock<ILogger<RefreshTokenHandler>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public RefreshTokenHandlerTests()
    {
        _tokenServiceMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<RefreshTokenHandler>>();
        _configurationMock = new Mock<IConfiguration>();

        // Setup default configuration
        _configurationMock.Setup(c => c["JWT:ExpirationMinutes"]).Returns("60");
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe"
        );
        dbContext.Users.Add(user);

        var refreshToken = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            User = user
        };
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("new-access-token");

        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("new-refresh-token");

        var handler = new RefreshTokenHandler(
            dbContext,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));

        // Verify old token was revoked
        var oldToken = await dbContext.RefreshTokens.FindAsync(refreshToken.Id);
        oldToken!.IsRevoked.Should().BeTrue();
        oldToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        oldToken.ReplacedByToken.Should().Be("new-refresh-token");
        oldToken.RevokedReason.Should().Be("Replaced by new token");

        // Verify new token was created
        var newToken = dbContext.RefreshTokens
            .FirstOrDefault(rt => rt.Token == "new-refresh-token");
        newToken.Should().NotBeNull();
        newToken!.UserId.Should().Be(user.Id);
        newToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_NonExistentToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();

        var handler = new RefreshTokenHandler(
            dbContext,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new RefreshTokenCommand("non-existent-token");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None)
        );

        exception.Message.Should().Be("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);

        var refreshToken = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
            CreatedAt = DateTime.UtcNow.AddDays(-31),
            User = user
        };
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        var handler = new RefreshTokenHandler(
            dbContext,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new RefreshTokenCommand("expired-token");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None)
        );

        exception.Message.Should().Be("Refresh token is no longer valid");
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsUnauthorizedAccessExceptionAndRevokesAllTokens()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);

        // Create a revoked token
        var revokedToken = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            RevokedAt = DateTime.UtcNow.AddHours(-1),
            RevokedReason = "Replaced by new token",
            User = user
        };

        // Create other active tokens for the same user
        var activeToken1 = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "active-token-1",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            User = user
        };

        var activeToken2 = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "active-token-2",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            User = user
        };

        dbContext.RefreshTokens.AddRange(revokedToken, activeToken1, activeToken2);
        await dbContext.SaveChangesAsync();

        var handler = new RefreshTokenHandler(
            dbContext,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new RefreshTokenCommand("revoked-token");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None)
        );

        exception.Message.Should().Be("Refresh token is no longer valid");

        // Verify all tokens were revoked (token reuse detection)
        var token1 = await dbContext.RefreshTokens.FindAsync(activeToken1.Id);
        var token2 = await dbContext.RefreshTokens.FindAsync(activeToken2.Id);

        token1!.IsRevoked.Should().BeTrue();
        token1.RevokedReason.Should().Be("Token reuse detected");
        token2!.IsRevoked.Should().BeTrue();
        token2.RevokedReason.Should().Be("Token reuse detected");
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(isActive: false);
        dbContext.Users.Add(user);

        var refreshToken = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            User = user
        };
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        var handler = new RefreshTokenHandler(
            dbContext,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new RefreshTokenCommand("valid-token");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None)
        );

        exception.Message.Should().Be("Account is inactive");
    }

    [Fact]
    public async Task Handle_TokenRotation_CreatesNewTokenAndRevokesOld()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);

        var oldTokenString = "old-refresh-token";
        var oldToken = new Hickory.Api.Infrastructure.Data.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = oldTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            User = user
        };
        dbContext.RefreshTokens.Add(oldToken);
        await dbContext.SaveChangesAsync();

        var newTokenString = "new-refresh-token";
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("new-access-token");
        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns(newTokenString);

        var handler = new RefreshTokenHandler(
            dbContext,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new RefreshTokenCommand(oldTokenString);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.RefreshToken.Should().Be(newTokenString);

        // Verify old token has ReplacedByToken field set
        var updatedOldToken = await dbContext.RefreshTokens.FindAsync(oldToken.Id);
        updatedOldToken!.ReplacedByToken.Should().Be(newTokenString);
        updatedOldToken.IsRevoked.Should().BeTrue();

        // Verify exactly 2 tokens exist: old (revoked) and new (active)
        var allTokens = dbContext.RefreshTokens.Where(rt => rt.UserId == user.Id).ToList();
        allTokens.Should().HaveCount(2);
        allTokens.Should().Contain(t => t.Token == oldTokenString && t.IsRevoked);
        allTokens.Should().Contain(t => t.Token == newTokenString && !t.IsRevoked);
    }
}
