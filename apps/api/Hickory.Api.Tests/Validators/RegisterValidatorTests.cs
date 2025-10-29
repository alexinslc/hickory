using FluentAssertions;
using Hickory.Api.Features.Auth.Models;
using Hickory.Api.Features.Auth.Register;

namespace Hickory.Api.Tests.Validators;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator;

    public RegisterValidatorTests()
    {
        _validator = new RegisterValidator();
    }

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPass123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmail_FailsValidation(string email)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = email,
            Password = "ValidPass123!",
            FirstName = "John",
            LastName = "Doe"
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
        var request = new RegisterRequest
        {
            Email = email,
            Password = "ValidPass123!",
            FirstName = "John",
            LastName = "Doe"
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
        var longEmail = new string('a', 250) + "@example.com";
        var request = new RegisterRequest
        {
            Email = longEmail,
            Password = "ValidPass123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email must not exceed 256 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyPassword_FailsValidation(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password is required");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_PasswordTooShort_FailsValidation(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password must be at least 8 characters");
    }

    [Theory]
    [InlineData("nouppercase1!")]
    [InlineData("alllowercase123!")]
    public void Validate_PasswordMissingUppercase_FailsValidation(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password must contain at least one uppercase letter");
    }

    [Theory]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("ALLUPPERCASE123!")]
    public void Validate_PasswordMissingLowercase_FailsValidation(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password must contain at least one lowercase letter");
    }

    [Theory]
    [InlineData("NoNumbers!")]
    [InlineData("OnlyLetters!")]
    public void Validate_PasswordMissingNumber_FailsValidation(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password must contain at least one number");
    }

    [Theory]
    [InlineData("NoSpecial1")]
    [InlineData("AlphaNumeric123")]
    public void Validate_PasswordMissingSpecialCharacter_FailsValidation(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password must contain at least one special character");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyFirstName_FailsValidation(string firstName)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPass123!",
            FirstName = firstName,
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName" && e.ErrorMessage == "First name is required");
    }

    [Fact]
    public void Validate_FirstNameTooLong_FailsValidation()
    {
        // Arrange
        var longFirstName = new string('a', 101);
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPass123!",
            FirstName = longFirstName,
            LastName = "Doe"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName" && e.ErrorMessage == "First name must not exceed 100 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyLastName_FailsValidation(string lastName)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPass123!",
            FirstName = "John",
            LastName = lastName
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName" && e.ErrorMessage == "Last name is required");
    }

    [Fact]
    public void Validate_LastNameTooLong_FailsValidation()
    {
        // Arrange
        var longLastName = new string('a', 101);
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPass123!",
            FirstName = "John",
            LastName = longLastName
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName" && e.ErrorMessage == "Last name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "invalid-email",
            Password = "weak",
            FirstName = "",
            LastName = ""
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(4);
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }
}
