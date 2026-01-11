using FluentValidation;

namespace Hickory.Api.Features.Attachments.Upload;

public class UploadAttachmentValidator : AbstractValidator<UploadAttachmentCommand>
{
    private static readonly string[] AllowedContentTypes = new[]
    {
        // Images
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml",
        // Documents
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation", // .pptx
        // Text
        "text/plain",
        "text/csv",
        "text/html",
        "application/json",
        "application/xml",
        "text/xml",
        // Archives
        "application/zip",
        "application/x-zip-compressed",
        "application/x-7z-compressed",
        "application/x-rar-compressed",
        // Other
        "application/octet-stream"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private const int MaxFileNameLength = 255;

    public UploadAttachmentValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("Ticket ID is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .MaximumLength(MaxFileNameLength)
            .WithMessage($"File name must not exceed {MaxFileNameLength} characters")
            .Must(HaveSafeFileName)
            .WithMessage("File name contains invalid characters");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required")
            .Must(BeAllowedContentType)
            .WithMessage("File type is not allowed");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024 / 1024}MB");

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File stream is required")
            .Must(stream => stream.CanRead)
            .WithMessage("File stream must be readable");

        RuleFor(x => x.UploadedById)
            .NotEmpty()
            .WithMessage("User ID is required");
    }

    private static bool BeAllowedContentType(string contentType)
    {
        return AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }

    private static bool HaveSafeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        // Check for invalid path characters (platform-specific)
        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.Any(c => invalidChars.Contains(c)))
        {
            return false;
        }

        // Additionally check for characters that are problematic on Windows
        // or in web contexts, regardless of platform
        char[] additionalInvalidChars = new[] { '<', '>', ':', '|', '?', '*' };
        if (fileName.Any(c => additionalInvalidChars.Contains(c)))
        {
            return false;
        }

        // Prevent directory traversal
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return false;
        }

        return true;
    }
}
