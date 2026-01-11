using MediatR;

namespace Hickory.Api.Features.Attachments.Download;

public record DownloadAttachmentQuery(
    Guid AttachmentId,
    Guid RequestingUserId
) : IRequest<DownloadAttachmentResponse>;

public record DownloadAttachmentResponse(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes
);
