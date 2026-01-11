using Hickory.Api.Features.Attachments.Delete;
using Hickory.Api.Features.Attachments.Download;
using Hickory.Api.Features.Attachments.Upload;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hickory.Api.Features.Attachments;

[ApiController]
[Route("api/attachments")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(IMediator mediator, ILogger<AttachmentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("tickets/{ticketId}")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public async Task<ActionResult<UploadAttachmentResponse>> UploadAttachment(
        Guid ticketId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        var userId = GetUserId();

        using var stream = file.OpenReadStream();
        var command = new UploadAttachmentCommand(
            ticketId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            userId);

        try
        {
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to upload attachment for ticket {TicketId}", ticketId);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadAttachment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var query = new DownloadAttachmentQuery(id, userId);

        try
        {
            var response = await _mediator.Send(query, cancellationToken);

            return File(
                response.FileStream,
                response.ContentType,
                response.FileName,
                enableRangeProcessing: true);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to download attachment {AttachmentId}", id);
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttachment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new DeleteAttachmentCommand(id, userId);

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete attachment {AttachmentId}", id);
            return NotFound(ex.Message);
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
