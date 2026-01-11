using MediatR;

namespace Hickory.Api.Features.Attachments.Delete;

public record DeleteAttachmentCommand(
    Guid AttachmentId,
    Guid RequestingUserId,
    string RequestingUserRole
) : IRequest;
