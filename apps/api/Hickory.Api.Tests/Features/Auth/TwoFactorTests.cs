using FluentAssertions;
using Hickory.Api.Features.Auth.Login;
using Hickory.Api.Features.Auth.TwoFactor;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hickory.Api.Tests.Features.Auth;

public class TwoFactorTests
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _tokenServiceMock;
    private readonly Mock<ILogger<LoginHandler>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public TwoFactorTests()
    {
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<LoginHandler>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["JWT:ExpirationMinutes"]).Returns("60");
    }

    [Fact]
    public async Task Handle_UserWith2FAEnabled_ReturnsTwoFactorRequired()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            passwordHash: "hashedPassword123"
        );
        
        // Enable 2FA for this user
        user.TwoFactorEnabled = true;
        user.TwoFactorSecret = "TESTSECRETKEY123456";
        user.TwoFactorEnabledAt = DateTime.UtcNow;
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        _passwordHasherMock
            .Setup(p => p.VerifyPassword("correctPassword", "hashedPassword123"))
            .Returns(true);

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
        result.RequiresTwoFactor.Should().BeTrue();
        result.AuthResponse.Should().BeNull();
        result.TwoFactorRequired.Should().NotBeNull();
        result.TwoFactorRequired!.UserId.Should().Be(user.Id);
        result.TwoFactorRequired.Email.Should().Be("test@example.com");
        result.TwoFactorRequired.Requires2FA.Should().BeTrue();
        
        // Tokens should NOT be generated yet
        _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _tokenServiceMock.Verify(t => t.GenerateRefreshToken(), Times.Never);
    }

    [Fact]
    public async Task Handle_UserWithout2FA_ReturnsAuthResponse()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "test@example.com",
            passwordHash: "hashedPassword123"
        );
        
        // 2FA is NOT enabled (default)
        user.TwoFactorEnabled = false;
        
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
        result.RequiresTwoFactor.Should().BeFalse();
        result.TwoFactorRequired.Should().BeNull();
        result.AuthResponse.Should().NotBeNull();
        result.AuthResponse!.AccessToken.Should().Be("mock-access-token");
    }
}

public class TwoFactorServiceTests
{
    private readonly TwoFactorService _service;

    public TwoFactorServiceTests()
    {
        _service = new TwoFactorService();
    }

    [Fact]
    public void GenerateSecretKey_ReturnsBase32String()
    {
        // Act
        var secret = _service.GenerateSecretKey();

        // Assert
        secret.Should().NotBeNullOrEmpty();
        secret.Should().MatchRegex("^[A-Z2-7]+$"); // Base32 charset
        secret.Length.Should().BeGreaterOrEqualTo(16); // Minimum length for security
    }

    [Fact]
    public void GenerateSecretKey_GeneratesUniqueKeys()
    {
        // Act
        var secrets = Enumerable.Range(0, 10)
            .Select(_ => _service.GenerateSecretKey())
            .ToList();

        // Assert
        secrets.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GenerateQrCodeUri_ReturnsValidOtpauthUri()
    {
        // Arrange
        var secret = _service.GenerateSecretKey();
        var email = "test@example.com";

        // Act
        var uri = _service.GenerateQrCodeUri(email, secret);

        // Assert
        uri.Should().StartWith("otpauth://totp/");
        uri.Should().Contain("secret=" + secret);
        uri.Should().Contain("issuer=Hickory");
        uri.Should().Contain(Uri.EscapeDataString(email));
    }

    [Fact]
    public void GenerateBackupCodes_ReturnsCorrectCount()
    {
        // Act
        var codes = _service.GenerateBackupCodes(10);

        // Assert
        codes.Should().HaveCount(10);
    }

    [Fact]
    public void GenerateBackupCodes_ReturnsFormattedCodes()
    {
        // Act
        var codes = _service.GenerateBackupCodes(5);

        // Assert
        foreach (var code in codes)
        {
            code.Should().MatchRegex("^[A-Z0-9]{4}-[A-Z0-9]{4}$");
        }
    }

    [Fact]
    public void GenerateBackupCodes_GeneratesUniqueCodes()
    {
        // Act
        var codes = _service.GenerateBackupCodes(100);

        // Assert
        codes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ValidateBackupCode_ValidCode_ReturnsTrueAndRemovesCode()
    {
        // Arrange
        var codes = _service.GenerateBackupCodes(5);
        var hashedCodes = TwoFactorService.HashBackupCodes(codes);
        var codeToUse = codes[2];

        // Act
        var result = _service.ValidateBackupCode(codeToUse, hashedCodes, out var updatedHashes);

        // Assert
        result.Should().BeTrue();
        
        // Verify the code can't be used again
        var secondResult = _service.ValidateBackupCode(codeToUse, updatedHashes, out _);
        secondResult.Should().BeFalse();
    }

    [Fact]
    public void ValidateBackupCode_InvalidCode_ReturnsFalse()
    {
        // Arrange
        var codes = _service.GenerateBackupCodes(5);
        var hashedCodes = TwoFactorService.HashBackupCodes(codes);

        // Act
        var result = _service.ValidateBackupCode("INVALID-CODE", hashedCodes, out var updatedHashes);

        // Assert
        result.Should().BeFalse();
        updatedHashes.Should().Be(hashedCodes); // Should not be modified
    }

    [Fact]
    public void ValidateBackupCode_CodeWithoutDash_StillWorks()
    {
        // Arrange
        var codes = _service.GenerateBackupCodes(5);
        var hashedCodes = TwoFactorService.HashBackupCodes(codes);
        var codeToUse = codes[0].Replace("-", ""); // Remove the dash

        // Act
        var result = _service.ValidateBackupCode(codeToUse, hashedCodes, out _);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateBackupCode_LowercaseCode_StillWorks()
    {
        // Arrange
        var codes = _service.GenerateBackupCodes(5);
        var hashedCodes = TwoFactorService.HashBackupCodes(codes);
        var codeToUse = codes[0].ToLowerInvariant();

        // Act
        var result = _service.ValidateBackupCode(codeToUse, hashedCodes, out _);

        // Assert
        result.Should().BeTrue();
    }
}
