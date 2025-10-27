using FluentValidation;
using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace Hickory.Api.Features.KnowledgeBase.Create;

public record CreateArticleCommand(CreateArticleRequest Request, Guid AuthorId) : IRequest<ArticleDto>;

public class CreateArticleValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(2).WithMessage("Title must be at least 2 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
        
        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(50000).WithMessage("Content must not exceed 50,000 characters");
        
        RuleFor(x => x.Request.Status)
            .IsInEnum().WithMessage("Invalid status value");
        
        RuleFor(x => x.Request.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("Maximum 10 tags allowed");
    }
}

public class CreateArticleHandler : IRequestHandler<CreateArticleCommand, ArticleDto>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateArticleHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArticleDto> Handle(CreateArticleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        // Validate category exists if provided
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId.Value, cancellationToken);
            
            if (!categoryExists)
            {
                throw new ValidationException("Category not found");
            }
        }

        // Create the article
        var article = new KnowledgeArticle
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            Status = request.Status,
            CategoryId = request.CategoryId,
            AuthorId = command.AuthorId,
            ViewCount = 0,
            HelpfulCount = 0,
            NotHelpfulCount = 0
        };

        // Set published date if status is Published
        if (article.Status == ArticleStatus.Published)
        {
            article.PublishedAt = DateTime.UtcNow;
        }

        // Generate search vector from title and content
        article.SearchVector = GenerateSearchVector(article.Title, article.Content);

        _dbContext.KnowledgeArticles.Add(article);

        // Handle tags if provided
        if (request.Tags.Any())
        {
            var tagNames = request.Tags.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            
            // Get existing tags
            var existingTags = await _dbContext.Tags
                .Where(t => tagNames.Contains(t.Name))
                .ToListAsync(cancellationToken);
            
            var existingTagNames = existingTags.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            
            // Create new tags for any that don't exist
            var newTagNames = tagNames.Where(n => !existingTagNames.Contains(n)).ToList();
            var newTags = newTagNames.Select(name => new Tag
            {
                Id = Guid.NewGuid(),
                Name = name
            }).ToList();
            
            if (newTags.Any())
            {
                _dbContext.Tags.AddRange(newTags);
            }
            
            // Combine existing and new tags
            var allTags = existingTags.Concat(newTags).ToList();
            
            // Create ArticleTag associations
            foreach (var tag in allTags)
            {
                article.ArticleTags.Add(new ArticleTag
                {
                    ArticleId = article.Id,
                    TagId = tag.Id
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Load related data for response
        var createdArticle = await _dbContext.KnowledgeArticles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .FirstAsync(a => a.Id == article.Id, cancellationToken);

        return MapToDto(createdArticle);
    }

    private static NpgsqlTsVector GenerateSearchVector(string title, string content)
    {
        // Combine title and content with weight to make title more relevant
        var searchText = $"{title} {title} {content}"; // Duplicate title for higher weight
        return NpgsqlTsVector.Parse(searchText);
    }

    private static ArticleDto MapToDto(KnowledgeArticle article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            Status = article.Status.ToString(),
            CategoryId = article.CategoryId,
            CategoryName = article.Category?.Name,
            Tags = article.ArticleTags.Select(at => at.Tag.Name).ToList(),
            ViewCount = article.ViewCount,
            HelpfulCount = article.HelpfulCount,
            NotHelpfulCount = article.NotHelpfulCount,
            AuthorId = article.AuthorId,
            AuthorName = $"{article.Author.FirstName} {article.Author.LastName}",
            LastUpdatedById = article.LastUpdatedById,
            LastUpdatedByName = article.LastUpdatedBy != null 
                ? $"{article.LastUpdatedBy.FirstName} {article.LastUpdatedBy.LastName}" 
                : null,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            PublishedAt = article.PublishedAt
        };
    }
}
