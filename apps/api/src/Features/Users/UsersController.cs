using Hickory.Api.Features.Users.DataExport;
using Hickory.Api.Features.Users.DeleteAccount;
using Hickory.Api.Features.Users.GetPreferences;
using Hickory.Api.Features.Users.UpdatePreferences;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hickory.Api.Features.Users;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get notification preferences for the current user
    /// </summary>
    [HttpGet("me/preferences")]
    public async Task<ActionResult<NotificationPreferencesDto>> GetMyPreferences()
    {
        var userId = GetUserId();
        var query = new GetNotificationPreferencesQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update notification preferences for the current user
    /// </summary>
    [HttpPut("me/preferences")]
    public async Task<ActionResult<NotificationPreferencesResponse>> UpdateMyPreferences([FromBody] UpdateNotificationPreferencesRequest request)
    {
        var userId = GetUserId();
        var command = new UpdateNotificationPreferencesCommand(userId, request);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Export all personal data for the current user (GDPR right to access)
    /// </summary>
    [HttpGet("me/data-export")]
    public async Task<ActionResult<UserDataExportDto>> ExportMyData()
    {
        var userId = GetUserId();
        var query = new ExportUserDataQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Delete the current user's account and anonymize personal data (GDPR right to erasure)
    /// </summary>
    [HttpDelete("me")]
    public async Task<ActionResult<DeleteAccountResponse>> DeleteMyAccount()
    {
        var userId = GetUserId();
        var command = new DeleteAccountCommand(userId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }
}
