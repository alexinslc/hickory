using FluentValidation.TestHelper;
using Hickory.Api.Features.Attachments.Upload;
using Moq;

namespace Hickory.Api.Tests.Features.Attachments;

public class UploadAttachmentValidatorTests
{
    private readonly UploadAttachmentValidator _validator;

    public UploadAttachmentValidatorTests()
    {
        _validator = new UploadAttachmentValidator();
    }

    [Fact]
    public void Should_Have_Error_When_FileName_Is_Empty()
    {
        // Arrange
        var stream = new MemoryStream(new byte[100]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            stream,
            string.Empty,
            "image/png",
            100,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FileName);
    }

    [Fact]
    public void Should_Have_Error_When_FileName_Is_Too_Long()
    {
        // Arrange
        var stream = new MemoryStream(new byte[100]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            stream,
            new string('a', 256),
            "image/png",
            100,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FileName);
    }

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("..\\..\\windows\\system32")]
    [InlineData("./../file.txt")]
    public void Should_Have_Error_When_FileName_Contains_Path_Traversal(string fileName)
    {
        // Arrange
        var stream = new MemoryStream(new byte[100]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            stream,
            fileName,
            "image/png",
            100,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FileName);
    }

    [Theory]
    [InlineData("file<.txt")]
    [InlineData("file>.txt")]
    [InlineData("file:.txt")]
    [InlineData("file|.txt")]
    [InlineData("file?.txt")]
    [InlineData("file*.txt")]
    public void Should_Have_Error_When_FileName_Contains_Invalid_Characters(string fileName)
    {
        // Arrange
        var stream = new MemoryStream(new byte[100]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            stream,
            fileName,
            "image/png",
            100,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FileName);
    }

    [Fact]
    public void Should_Have_Error_When_ContentType_Is_Not_Allowed()
    {
        // Arrange
        var stream = new MemoryStream(new byte[100]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            stream,
            "file.exe",
            "application/x-msdownload",
            100,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ContentType);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("application/pdf")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("text/plain")]
    public void Should_Not_Have_Error_When_ContentType_Is_Allowed(string contentType)
    {
        // Arrange
        var stream = new MemoryStream(new byte[100]);
        stream.WriteByte(1); // Make stream readable
        stream.Position = 0;
        
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            stream,
            "file.txt",
            contentType,
            100,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.ContentType);
    }

    [Fact]
    public void Should_Have_Error_When_FileSize_Exceeds_Limit()
    {
        // Arrange
        var stream = new MemoryStream(new byte[100]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            stream,
            "large-file.pdf",
            "application/pdf",
            11 * 1024 * 1024, // 11 MB
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FileSizeBytes);
    }

    [Fact]
    public void Should_Have_Error_When_FileSize_Is_Zero()
    {
        // Arrange
        var stream = new MemoryStream(new byte[0]);
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            stream,
            "empty-file.txt",
            "text/plain",
            0,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FileSizeBytes);
    }

    [Fact]
    public void Should_Have_Error_When_Stream_Is_Not_Readable()
    {
        // Arrange
        var mockStream = new Mock<Stream>();
        mockStream.Setup(s => s.CanRead).Returns(false);
        
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            mockStream.Object,
            "file.txt",
            "text/plain",
            100,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FileStream);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var stream = new MemoryStream(new byte[100]);
        stream.WriteByte(1); // Make stream readable
        stream.Position = 0;
        
        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            stream,
            "valid-file.pdf",
            "application/pdf",
            1024 * 1024, // 1 MB
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
