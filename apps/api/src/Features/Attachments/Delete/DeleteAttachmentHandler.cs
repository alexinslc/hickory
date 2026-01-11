using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Attachments.Delete;

public class DeleteAttachmentHandler : IRequestHandler<DeleteAttachmentCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteAttachmentHandler> _logger;

    public DeleteAttachmentHandler(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        ILogger<DeleteAttachmentHandler> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _context.Attachments
            .Include(a => a.Ticket)
            .Include(a => a.Comment)
                .ThenInclude(c => c!.Ticket)
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, cancellationToken);

        if (attachment == null)
        {
            throw new InvalidOperationException($"Attachment {request.AttachmentId} not found");
        }

        // Access control check
        var isAdmin = request.RequestingUserRole == "Admin";
        var isAgent = request.RequestingUserRole == "Agent" || isAdmin;
        var isUploader = attachment.UploadedById == request.RequestingUserId;
        var ticket = attachment.Comment?.Ticket ?? attachment.Ticket;
        var isAssignedAgent = ticket.AssignedToId == request.RequestingUserId;

        if (!isAdmin && !isUploader && !isAssignedAgent)
        {
            _logger.LogWarning("User {UserId} attempted to delete attachment {AttachmentId} without permission",
                request.RequestingUserId, request.AttachmentId);
            throw new UnauthorizedAccessException("You do not have permission to delete this attachment");
        }

        // Delete from storage first
        try
        {
            await _fileStorageService.DeleteFileAsync(attachment.StoragePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file from storage for attachment {AttachmentId}", request.AttachmentId);
            // Continue with database deletion even if file deletion fails
        }

        // Delete from database
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} deleted attachment {AttachmentId}", 
            request.RequestingUserId, request.AttachmentId);
    }
}
