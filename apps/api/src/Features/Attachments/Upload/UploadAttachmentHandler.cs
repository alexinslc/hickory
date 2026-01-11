using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Infrastructure.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Attachments.Upload;

public class UploadAttachmentHandler : IRequestHandler<UploadAttachmentCommand, UploadAttachmentResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadAttachmentHandler> _logger;

    public UploadAttachmentHandler(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        ILogger<UploadAttachmentHandler> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<UploadAttachmentResponse> Handle(
        UploadAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Verify ticket exists and user has access
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new InvalidOperationException($"Ticket {request.TicketId} not found");
        }

        // Authorization: only ticket owners, assigned agents, or admins can upload attachments
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UploadedById, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User {request.UploadedById} not found");
        }

        var isOwner = ticket.SubmitterId == request.UploadedById;
        var isAssignedAgent = ticket.AssignedToId == request.UploadedById;
        var isAdmin = user.Role == UserRole.Administrator;

        if (!isOwner && !isAssignedAgent && !isAdmin)
        {
            _logger.LogWarning(
                "Unauthorized attachment upload attempt by user {UserId} for ticket {TicketId}",
                request.UploadedById,
                request.TicketId);
            throw new UnauthorizedAccessException("You do not have permission to upload attachments to this ticket.");
        }

        // Upload file to storage
        string storagePath;
        try
        {
            storagePath = await _fileStorageService.UploadFileAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName} for ticket {TicketId}", 
                request.FileName, request.TicketId);
            throw new InvalidOperationException("Failed to upload file", ex);
        }

        // Create attachment record
        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSizeBytes,
            StoragePath = storagePath,
            TicketId = request.TicketId,
            UploadedById = request.UploadedById,
            UploadedAt = DateTime.UtcNow
        };

        _context.Attachments.Add(attachment);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // If database save fails, try to clean up the uploaded file
            _logger.LogError(ex, "Failed to save attachment record for {FileName}", request.FileName);
            
            try
            {
                await _fileStorageService.DeleteFileAsync(storagePath, cancellationToken);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning(cleanupEx, "Failed to cleanup file {StoragePath} after database error", storagePath);
            }
            
            throw new InvalidOperationException("Failed to save attachment", ex);
        }

        _logger.LogInformation("Successfully uploaded attachment {AttachmentId} for ticket {TicketId}", 
            attachment.Id, request.TicketId);

        return new UploadAttachmentResponse(
            attachment.Id,
            attachment.FileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.UploadedAt,
            attachment.UploadedById);
    }
}
