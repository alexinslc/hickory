using FluentValidation;
using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace Hickory.Api.Features.KnowledgeBase.Update;

public record UpdateArticleCommand(Guid ArticleId, UpdateArticleRequest Request, Guid UpdatedById) : IRequest<ArticleDto>;

public class UpdateArticleValidator : AbstractValidator<UpdateArticleCommand>
{
    public UpdateArticleValidator()
    {
        RuleFor(x => x.Request.Title)
            .MinimumLength(2).WithMessage("Title must be at least 2 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => x.Request.Title != null);
        
        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Content cannot be empty")
            .MaximumLength(50000).WithMessage("Content must not exceed 50,000 characters")
            .When(x => x.Request.Content != null);
        
        RuleFor(x => x.Request.Status)
            .IsInEnum().WithMessage("Invalid status value")
            .When(x => x.Request.Status.HasValue);
        
        RuleFor(x => x.Request.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("Maximum 10 tags allowed");
    }
}

public class UpdateArticleHandler : IRequestHandler<UpdateArticleCommand, ArticleDto>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateArticleHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArticleDto> Handle(UpdateArticleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        // Load article with related data
        var article = await _dbContext.KnowledgeArticles
            .Include(a => a.ArticleTags)
            .FirstOrDefaultAsync(a => a.Id == command.ArticleId, cancellationToken);

        if (article == null)
        {
            throw new KeyNotFoundException($"Article with ID {command.ArticleId} not found");
        }

        // Update fields if provided
        var needsSearchVectorUpdate = false;
        
        if (request.Title != null && request.Title != article.Title)
        {
            article.Title = request.Title;
            needsSearchVectorUpdate = true;
        }
        
        if (request.Content != null && request.Content != article.Content)
        {
            article.Content = request.Content;
            needsSearchVectorUpdate = true;
        }
        
        if (request.Status.HasValue && request.Status.Value != article.Status)
        {
            var oldStatus = article.Status;
            article.Status = request.Status.Value;
            
            // Set published date when transitioning to Published status
            if (oldStatus != ArticleStatus.Published && article.Status == ArticleStatus.Published)
            {
                article.PublishedAt = DateTime.UtcNow;
            }
        }
        
        if (request.CategoryId.HasValue)
        {
            // Validate category exists if provided and not null
            if (request.CategoryId.Value != Guid.Empty)
            {
                var categoryExists = await _dbContext.Categories
                    .AnyAsync(c => c.Id == request.CategoryId.Value, cancellationToken);
                
                if (!categoryExists)
                {
                    throw new ValidationException("Category not found");
                }
                
                article.CategoryId = request.CategoryId.Value;
            }
            else
            {
                // Empty GUID means remove category
                article.CategoryId = null;
            }
        }

        // Update search vector if title or content changed
        if (needsSearchVectorUpdate)
        {
            article.SearchVector = GenerateSearchVector(article.Title, article.Content);
        }

        // Track who updated the article
        article.LastUpdatedById = command.UpdatedById;

        // Handle tags if provided
        if (request.Tags != null)
        {
            // Remove all existing tag associations
            _dbContext.RemoveRange(article.ArticleTags);
            article.ArticleTags.Clear();

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
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Reload with all related data for response
        var updatedArticle = await _dbContext.KnowledgeArticles
            .Include(a => a.Author)
            .Include(a => a.LastUpdatedBy)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .FirstAsync(a => a.Id == article.Id, cancellationToken);

        return MapToDto(updatedArticle);
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
