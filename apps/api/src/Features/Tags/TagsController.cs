using MediatR;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Hickory.Api.Features.Tags.GetAll;

namespace Hickory.Api.Features.Tags;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all tags ordered alphabetically
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        var query = new GetAllTagsQuery();
        var tags = await _mediator.Send(query);
        return Ok(tags);
    }
}
