using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.KnowledgeBase.Rate;

public record RateArticleCommand(Guid ArticleId, bool IsHelpful) : IRequest<Unit>;

public class RateArticleHandler : IRequestHandler<RateArticleCommand, Unit>
{
    private readonly ApplicationDbContext _dbContext;

    public RateArticleHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(RateArticleCommand command, CancellationToken cancellationToken)
    {
        var article = await _dbContext.KnowledgeArticles
            .FirstOrDefaultAsync(a => a.Id == command.ArticleId, cancellationToken);

        if (article == null)
        {
            throw new KeyNotFoundException($"Article with ID {command.ArticleId} not found");
        }

        // Increment the appropriate counter
        if (command.IsHelpful)
        {
            article.HelpfulCount++;
        }
        else
        {
            article.NotHelpfulCount++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
