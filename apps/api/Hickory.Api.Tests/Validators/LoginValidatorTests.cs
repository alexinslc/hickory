using FluentAssertions;
using Hickory.Api.Features.Auth.Login;
using Hickory.Api.Features.Auth.Models;

namespace Hickory.Api.Tests.Validators;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator;

    public LoginValidatorTests()
    {
        _validator = new LoginValidator();
    }

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyEmail_FailsValidation(string email)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test")]
    public void Validate_InvalidEmailFormat_FailsValidation(string email)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Invalid email format");
    }

    [Fact]
    public void Validate_EmailTooLong_FailsValidation()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // > 256 characters
        var request = new LoginRequest
        {
            Email = longEmail,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email must not exceed 256 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyPassword_FailsValidation(string password)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = password
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password is required");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]  // 7 characters
    public void Validate_PasswordTooShort_FailsValidation(string password)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = password
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password must be at least 8 characters");
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "invalid-email",
            Password = "short"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(2);
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
