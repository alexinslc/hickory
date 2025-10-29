using FluentAssertions;
using Hickory.Api.Features.Tickets.Create;
using Hickory.Api.Features.Tickets.Create.Models;

namespace Hickory.Api.Tests.Validators;

public class CreateTicketValidatorTests
{
    private readonly CreateTicketValidator _validator;

    public CreateTicketValidatorTests()
    {
        _validator = new CreateTicketValidator();
    }

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = "Valid ticket title",
            Description = "This is a valid description with enough characters",
            Priority = "Medium"
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
    public void Validate_EmptyTitle_FailsValidation(string title)
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = title,
            Description = "Valid description here",
            Priority = "Medium"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage == "Title is required");
    }

    [Theory]
    [InlineData("Test")]
    [InlineData("1234")]
    public void Validate_TitleTooShort_FailsValidation(string title)
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = title,
            Description = "Valid description here",
            Priority = "Medium"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage == "Title must be at least 5 characters");
    }

    [Fact]
    public void Validate_TitleTooLong_FailsValidation()
    {
        // Arrange
        var longTitle = new string('a', 201);
        var request = new CreateTicketRequest
        {
            Title = longTitle,
            Description = "Valid description here",
            Priority = "Medium"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage == "Title cannot exceed 200 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyDescription_FailsValidation(string description)
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = "Valid ticket title",
            Description = description,
            Priority = "Medium"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage == "Description is required");
    }

    [Theory]
    [InlineData("Short")]
    [InlineData("12345678")]
    public void Validate_DescriptionTooShort_FailsValidation(string description)
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = "Valid ticket title",
            Description = description,
            Priority = "Medium"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage == "Description must be at least 10 characters");
    }

    [Fact]
    public void Validate_DescriptionTooLong_FailsValidation()
    {
        // Arrange
        var longDescription = new string('a', 10001);
        var request = new CreateTicketRequest
        {
            Title = "Valid ticket title",
            Description = longDescription,
            Priority = "Medium"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 10000 characters");
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    [InlineData("Critical")]
    public void Validate_ValidPriority_PassesValidation(string priority)
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = "Valid ticket title",
            Description = "Valid description here",
            Priority = priority
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Urgent")]
    [InlineData("Normal")]
    [InlineData("")]
    public void Validate_InvalidPriority_FailsValidation(string priority)
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = "Valid ticket title",
            Description = "Valid description here",
            Priority = priority
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority" && e.ErrorMessage == "Priority must be one of: Low, Medium, High, Critical");
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = "",
            Description = "short",
            Priority = "Invalid"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(3);
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }
}
