using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Attachments.Download;

public class DownloadAttachmentHandler : IRequestHandler<DownloadAttachmentQuery, DownloadAttachmentResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DownloadAttachmentHandler> _logger;

    public DownloadAttachmentHandler(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        ILogger<DownloadAttachmentHandler> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<DownloadAttachmentResponse> Handle(
        DownloadAttachmentQuery request,
        CancellationToken cancellationToken)
    {
        // Get attachment with ticket info for access control
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
        var isAgent = request.RequestingUserRole == "Agent" || request.RequestingUserRole == "Admin";
        var ticket = attachment.Comment?.Ticket ?? attachment.Ticket;
        var isTicketOwner = ticket.SubmitterId == request.RequestingUserId;
        var isAssignedAgent = ticket.AssignedToId == request.RequestingUserId;

        if (!isAgent && !isTicketOwner && !isAssignedAgent)
        {
            _logger.LogWarning("User {UserId} attempted to download attachment {AttachmentId} without permission",
                request.RequestingUserId, request.AttachmentId);
            throw new UnauthorizedAccessException("You do not have permission to download this attachment");
        }

        try
        {
            var fileStream = await _fileStorageService.DownloadFileAsync(
                attachment.StoragePath,
                cancellationToken);

            _logger.LogInformation("User {UserId} downloaded attachment {AttachmentId}", 
                request.RequestingUserId, request.AttachmentId);

            return new DownloadAttachmentResponse(
                fileStream,
                attachment.FileName,
                attachment.ContentType,
                attachment.FileSizeBytes);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found in storage for attachment {AttachmentId}", request.AttachmentId);
            throw new InvalidOperationException("File not found in storage", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download attachment {AttachmentId}", request.AttachmentId);
            throw new InvalidOperationException("Failed to download file", ex);
        }
    }
}
