using FluentAssertions;
using Hickory.Api.Features.KnowledgeBase.Models;
using Hickory.Api.Features.KnowledgeBase.Search;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Tests.Features.KnowledgeBase;

public class SearchArticlesHandlerTests
{
    [Fact]
    public async Task Handle_WithoutSearchQuery_ReturnsPublishedArticlesOrderedByPublishedDate()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        var article1 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "First Article",
            publishedAt: DateTime.UtcNow.AddDays(-2));
        
        var article2 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Recent Article",
            publishedAt: DateTime.UtcNow.AddDays(-1));

        dbContext.KnowledgeArticles.AddRange(article1, article2);
        await dbContext.SaveChangesAsync();

        var handler = new SearchArticlesHandler(dbContext);
        var query = new SearchArticlesQuery(
            SearchQuery: null,
            CategoryId: null,
            Tags: null,
            Status: null,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Articles.Should().HaveCount(2);
        result.Articles[0].Title.Should().Be("Recent Article"); // Most recent first
        result.Articles[1].Title.Should().Be("First Article");
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyArticlesWithStatus()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        var publishedArticle = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Published Article",
            status: ArticleStatus.Published);
        
        var draftArticle = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Draft Article",
            status: ArticleStatus.Draft);

        dbContext.KnowledgeArticles.AddRange(publishedArticle, draftArticle);
        await dbContext.SaveChangesAsync();

        var handler = new SearchArticlesHandler(dbContext);
        var query = new SearchArticlesQuery(
            SearchQuery: null,
            CategoryId: null,
            Tags: null,
            Status: ArticleStatus.Draft,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Articles.Should().HaveCount(1);
        result.Articles[0].Title.Should().Be("Draft Article");
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ReturnsOnlyArticlesInCategory()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        var category1 = TestDataBuilder.CreateTestCategory(name: "Category 1");
        var category2 = TestDataBuilder.CreateTestCategory(name: "Category 2");
        
        dbContext.Users.Add(author);
        dbContext.Categories.AddRange(category1, category2);

        var article1 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article in Category 1",
            categoryId: category1.Id);
        
        var article2 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article in Category 2",
            categoryId: category2.Id);

        dbContext.KnowledgeArticles.AddRange(article1, article2);
        await dbContext.SaveChangesAsync();

        var handler = new SearchArticlesHandler(dbContext);
        var query = new SearchArticlesQuery(
            SearchQuery: null,
            CategoryId: category1.Id,
            Tags: null,
            Status: null,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Articles.Should().HaveCount(1);
        result.Articles[0].Title.Should().Be("Article in Category 1");
    }

    [Fact]
    public async Task Handle_WithTagFilter_ReturnsOnlyArticlesWithTags()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        var tag1 = TestDataBuilder.CreateTestTag("tag1");
        var tag2 = TestDataBuilder.CreateTestTag("tag2");
        
        dbContext.Users.Add(author);
        dbContext.Tags.AddRange(tag1, tag2);

        var article1 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article with tag1");
        article1.Tags.Add(tag1);
        
        var article2 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article with tag2");
        article2.Tags.Add(tag2);

        dbContext.KnowledgeArticles.AddRange(article1, article2);
        await dbContext.SaveChangesAsync();

        var handler = new SearchArticlesHandler(dbContext);
        var query = new SearchArticlesQuery(
            SearchQuery: null,
            CategoryId: null,
            Tags: new List<string> { "tag1" },
            Status: null,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Articles.Should().HaveCount(1);
        result.Articles[0].Title.Should().Be("Article with tag1");
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        // Create 5 articles
        for (int i = 1; i <= 5; i++)
        {
            var article = TestDataBuilder.CreateTestKnowledgeArticle(
                authorId: author.Id,
                title: $"Article {i}",
                publishedAt: DateTime.UtcNow.AddDays(-i));
            dbContext.KnowledgeArticles.Add(article);
        }
        await dbContext.SaveChangesAsync();

        var handler = new SearchArticlesHandler(dbContext);
        var query = new SearchArticlesQuery(
            SearchQuery: null,
            CategoryId: null,
            Tags: null,
            Status: null,
            Page: 1,
            PageSize: 2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Articles.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_DefaultsToPublishedArticles_WhenNoStatusSpecified()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        var publishedArticle = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Published Article",
            status: ArticleStatus.Published);
        
        var draftArticle = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Draft Article",
            status: ArticleStatus.Draft);

        var archivedArticle = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Archived Article",
            status: ArticleStatus.Archived);

        dbContext.KnowledgeArticles.AddRange(publishedArticle, draftArticle, archivedArticle);
        await dbContext.SaveChangesAsync();

        var handler = new SearchArticlesHandler(dbContext);
        var query = new SearchArticlesQuery(
            SearchQuery: null,
            CategoryId: null,
            Tags: null,
            Status: null, // Not specifying status
            Page: 1,
            PageSize: 20);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Articles.Should().HaveCount(1);
        result.Articles[0].Title.Should().Be("Published Article");
    }
}
