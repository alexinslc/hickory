using FluentAssertions;
using Hickory.Api.Features.Auth.Login;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hickory.Api.Tests.Features.Auth;

public class LoginHandlerTests
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _tokenServiceMock;
    private readonly Mock<ILogger<LoginHandler>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public LoginHandlerTests()
    {
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<LoginHandler>>();
        _configurationMock = new Mock<IConfiguration>();

        // Setup default configuration
        _configurationMock.Setup(c => c["JWT:ExpirationMinutes"]).Returns("60");
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            role: UserRole.EndUser,
            passwordHash: "hashedPassword123"
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        _passwordHasherMock
            .Setup(p => p.VerifyPassword("correctPassword", "hashedPassword123"))
            .Returns(true);

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("mock-access-token");

        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("mock-refresh-token");

        var handler = new LoginHandler(
            dbContext,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new LoginCommand("test@example.com", "correctPassword");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock-access-token");
        result.RefreshToken.Should().Be("mock-refresh-token");
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Role.Should().Be("EndUser");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));

        // Verify user's last login was updated
        var updatedUser = await dbContext.Users.FindAsync(user.Id);
        updatedUser!.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();

        var handler = new LoginHandler(
            dbContext,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new LoginCommand("nonexistent@example.com", "password");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None)
        );

        exception.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "test@example.com",
            passwordHash: "hashedPassword123"
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        _passwordHasherMock
            .Setup(p => p.VerifyPassword("wrongPassword", "hashedPassword123"))
            .Returns(false);

        var handler = new LoginHandler(
            dbContext,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new LoginCommand("test@example.com", "wrongPassword");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None)
        );

        exception.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "inactive@example.com",
            isActive: false,
            passwordHash: "hashedPassword123"
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new LoginHandler(
            dbContext,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new LoginCommand("inactive@example.com", "password");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None)
        );

        exception.Message.Should().Be("Account is inactive");
    }

    [Fact]
    public async Task Handle_NullPasswordHash_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "test@example.com",
            passwordHash: null
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new LoginHandler(
            dbContext,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new LoginCommand("test@example.com", "password");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None)
        );

        exception.Message.Should().Be("Invalid email or password");
    }

    [Theory]
    [InlineData(UserRole.EndUser)]
    [InlineData(UserRole.Agent)]
    [InlineData(UserRole.Administrator)]
    public async Task Handle_DifferentRoles_ReturnsCorrectRole(UserRole role)
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            role: role,
            passwordHash: "hashedPassword123"
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        _passwordHasherMock
            .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("mock-access-token");

        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("mock-refresh-token");

        var handler = new LoginHandler(
            dbContext,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object
        );

        var command = new LoginCommand(user.Email, "password");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Should().Be(role.ToString());
    }
}
