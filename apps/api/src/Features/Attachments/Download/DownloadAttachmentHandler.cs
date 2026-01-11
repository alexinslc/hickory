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
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, cancellationToken);

        if (attachment == null)
        {
            throw new InvalidOperationException($"Attachment {request.AttachmentId} not found");
        }

        // TODO: Add proper access control check
        // For now, just verify the ticket exists
        // In production, check if user has access to the ticket:
        // - User is ticket submitter
        // - User is assigned agent
        // - User is admin
        // - User has appropriate role/permission

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
