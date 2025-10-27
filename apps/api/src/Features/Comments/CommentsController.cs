using Hickory.Api.Features.Comments.Create;
using Hickory.Api.Features.Tickets.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hickory.Api.Features.Comments;

[ApiController]
[Route("api/tickets/{ticketId}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment(
        Guid ticketId,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userRole = GetUserRole();
        var command = new AddCommentCommand(ticketId, request, userId, userRole);
        
        try
        {
            var comment = await _mediator.Send(command, cancellationToken);
            return Ok(comment);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        return userId;
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value 
            ?? User.FindFirst("role")?.Value 
            ?? "User";
    }
}
