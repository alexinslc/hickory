using FluentAssertions;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Hickory.Api.Tests.Infrastructure.Auth;

public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<JwtTokenService>> _loggerMock;
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<JwtTokenService>>();

        // Setup default JWT configuration
        _configurationMock.Setup(c => c["JWT:Secret"]).Returns("ThisIsAVerySecretKeyForTestingPurposesOnly123!");
        _configurationMock.Setup(c => c["JWT:Issuer"]).Returns("HickoryTestIssuer");
        _configurationMock.Setup(c => c["JWT:Audience"]).Returns("HickoryTestAudience");
        _configurationMock.Setup(c => c["JWT:ExpirationMinutes"]).Returns("60");

        _service = new JwtTokenService(_configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_ValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = TestDataBuilder.CreateTestUser(
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            role: Hickory.Api.Infrastructure.Data.Entities.UserRole.EndUser
        );

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Decode and verify token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.GivenName && c.Value == "John");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.FamilyName && c.Value == "Doe");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "EndUser");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iat);
    }

    [Fact]
    public void GenerateAccessToken_ValidUser_HasCorrectExpiration()
    {
        // Arrange
        var user = TestDataBuilder.CreateTestUser();

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_MissingSecret_ThrowsInvalidOperationException()
    {
        // Arrange
        _configurationMock.Setup(c => c["JWT:Secret"]).Returns((string?)null);
        var service = new JwtTokenService(_configurationMock.Object, _loggerMock.Object);
        var user = TestDataBuilder.CreateTestUser();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => service.GenerateAccessToken(user));
        exception.Message.Should().Be("JWT:Secret not configured");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Length.Should().BeGreaterThan(0);

        // Verify it's a valid base64 string
        var bytes = Convert.FromBase64String(refreshToken);
        bytes.Length.Should().Be(32);
    }

    [Fact]
    public void GenerateRefreshToken_GeneratesUniqueTokens()
    {
        // Act
        var token1 = _service.GenerateRefreshToken();
        var token2 = _service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var user = TestDataBuilder.CreateTestUser(
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe"
        );
        var token = _service.GenerateAccessToken(user);

        // Act
        var principal = _service.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
        principal.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _service.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ExpiredToken_ReturnsNull()
    {
        // Arrange
        _configurationMock.Setup(c => c["JWT:ExpirationMinutes"]).Returns("-1"); // Already expired
        var service = new JwtTokenService(_configurationMock.Object, _loggerMock.Object);
        var user = TestDataBuilder.CreateTestUser();
        var token = service.GenerateAccessToken(user);

        // Act
        var principal = service.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WrongIssuer_ReturnsNull()
    {
        // Arrange
        var user = TestDataBuilder.CreateTestUser();
        var token = _service.GenerateAccessToken(user);

        // Create a new service with different issuer
        _configurationMock.Setup(c => c["JWT:Issuer"]).Returns("DifferentIssuer");
        var differentService = new JwtTokenService(_configurationMock.Object, _loggerMock.Object);

        // Act
        var principal = differentService.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }
}
