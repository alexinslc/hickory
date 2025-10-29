using FluentAssertions;
using Hickory.Api.Features.Comments.Create;

namespace Hickory.Api.Tests.Validators;

public class AddCommentValidatorTests
{
    private readonly AddCommentValidator _validator;

    public AddCommentValidatorTests()
    {
        _validator = new AddCommentValidator();
    }

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        // Arrange
        var request = new AddCommentRequest
        {
            Content = "This is a valid comment with good content.",
            IsInternal = false
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidInternalComment_PassesValidation()
    {
        // Arrange
        var request = new AddCommentRequest
        {
            Content = "This is a valid internal note.",
            IsInternal = true
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
    public void Validate_EmptyContent_FailsValidation(string content)
    {
        // Arrange
        var request = new AddCommentRequest
        {
            Content = content,
            IsInternal = false
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Content" && e.ErrorMessage == "Comment content is required");
    }

    [Fact]
    public void Validate_ContentTooLong_FailsValidation()
    {
        // Arrange
        var longContent = new string('a', 5001);
        var request = new AddCommentRequest
        {
            Content = longContent,
            IsInternal = false
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Content" && e.ErrorMessage == "Comment cannot exceed 5000 characters");
    }

    [Fact]
    public void Validate_ContentAtMaximumLength_PassesValidation()
    {
        // Arrange
        var maxContent = new string('a', 5000);
        var request = new AddCommentRequest
        {
            Content = maxContent,
            IsInternal = false
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShortContent_PassesValidation()
    {
        // Arrange
        var request = new AddCommentRequest
        {
            Content = "OK",
            IsInternal = false
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_SingleCharacterContent_PassesValidation()
    {
        // Arrange
        var request = new AddCommentRequest
        {
            Content = "a",
            IsInternal = false
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ContentWithSpecialCharacters_PassesValidation()
    {
        // Arrange
        var request = new AddCommentRequest
        {
            Content = "This comment has special chars: @#$%^&*()!",
            IsInternal = false
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ContentWithNewlines_PassesValidation()
    {
        // Arrange
        var request = new AddCommentRequest
        {
            Content = "Line 1\nLine 2\nLine 3",
            IsInternal = false
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
