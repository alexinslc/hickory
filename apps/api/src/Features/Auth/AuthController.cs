using Hickory.Api.Features.Auth.Login;
using Hickory.Api.Features.Auth.Models;
using Hickory.Api.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hickory.Api.Features.Auth;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand(request.Email, request.Password);
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName);
            
            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(Register), new { id = response.UserId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
