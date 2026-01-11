using MediatR;

namespace Hickory.Api.Features.Attachments.Upload;

public record UploadAttachmentCommand(
    Guid TicketId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Guid UploadedById
) : IRequest<UploadAttachmentResponse>;

public record UploadAttachmentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    DateTime UploadedAt
);
