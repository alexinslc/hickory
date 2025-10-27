using Hickory.Api.Features.Tags.Create;
using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tags.GetAll;

public record GetAllTagsQuery : IRequest<List<TagDto>>;

public class GetAllTagsHandler : IRequestHandler<GetAllTagsQuery, List<TagDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAllTagsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TagDto>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _dbContext.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return tags;
    }
}
