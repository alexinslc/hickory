using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Tags.Create;

public record CreateTagCommand(
    string Name,
    string? Color
) : IRequest<TagDto>;

public class CreateTagHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateTagHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        // Auto-create on first use - check if tag already exists
        var normalizedName = request.Name.ToLowerInvariant();
        var existingTag = await _dbContext.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedName, cancellationToken);

        if (existingTag != null)
        {
            return new TagDto
            {
                Id = existingTag.Id,
                Name = existingTag.Name,
                Color = existingTag.Color,
                CreatedAt = existingTag.CreatedAt
            };
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Color = request.Color
        };

        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            CreatedAt = tag.CreatedAt
        };
    }
}

public record TagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
    public DateTime CreatedAt { get; init; }
}
