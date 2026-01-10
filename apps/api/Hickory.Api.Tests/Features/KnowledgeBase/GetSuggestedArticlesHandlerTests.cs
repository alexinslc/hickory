using FluentAssertions;
using Hickory.Api.Features.KnowledgeBase.GetSuggested;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;

namespace Hickory.Api.Tests.Features.KnowledgeBase;

public class GetSuggestedArticlesHandlerTests
{
    [Fact]
    public async Task Handle_WithCategoryId_ReturnsMostHelpfulArticlesInCategory()
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
            title: "Helpful Article in Category 1",
            categoryId: category1.Id,
            helpfulCount: 10);
        
        var article2 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Less Helpful Article in Category 1",
            categoryId: category1.Id,
            helpfulCount: 5);

        var article3 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article in Category 2",
            categoryId: category2.Id,
            helpfulCount: 20);

        dbContext.KnowledgeArticles.AddRange(article1, article2, article3);
        await dbContext.SaveChangesAsync();

        var handler = new GetSuggestedArticlesHandler(dbContext);
        var query = new GetSuggestedArticlesQuery(
            TicketTitle: null,
            TicketDescription: null,
            CategoryId: category1.Id,
            Tags: null,
            Limit: 2); // Matches the count in category, so should return only category articles

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Helpful Article in Category 1");
        result[1].Title.Should().Be("Less Helpful Article in Category 1");
    }

    [Fact]
    public async Task Handle_WithTags_ReturnsArticlesWithMatchingTags()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        var tag1 = TestDataBuilder.CreateTestTag("tag1");
        var tag2 = TestDataBuilder.CreateTestTag("tag2");
        var tag3 = TestDataBuilder.CreateTestTag("tag3");
        
        dbContext.Users.Add(author);
        dbContext.Tags.AddRange(tag1, tag2, tag3);

        var article1 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article with tag1 and tag2",
            helpfulCount: 5);
        article1.Tags.Add(tag1);
        article1.Tags.Add(tag2);
        
        var article2 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article with only tag1",
            helpfulCount: 10);
        article2.Tags.Add(tag1);

        var article3 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article with tag3",
            helpfulCount: 15);
        article3.Tags.Add(tag3);

        dbContext.KnowledgeArticles.AddRange(article1, article2, article3);
        await dbContext.SaveChangesAsync();

        var handler = new GetSuggestedArticlesHandler(dbContext);
        var query = new GetSuggestedArticlesQuery(
            TicketTitle: null,
            TicketDescription: null,
            CategoryId: null,
            Tags: new List<string> { "tag1", "tag2" },
            Limit: 2); // Should match the count of articles with these tags

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Article with tag1 and tag2"); // More matching tags
        result[1].Title.Should().Be("Article with only tag1");
    }

    [Fact]
    public async Task Handle_WithNoMatchingFilters_ReturnsMostHelpfulArticles()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        var article1 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Most Helpful Article",
            helpfulCount: 20,
            viewCount: 100);
        
        var article2 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Less Helpful Article",
            helpfulCount: 10,
            viewCount: 50);

        var article3 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Least Helpful Article",
            helpfulCount: 5,
            viewCount: 25);

        dbContext.KnowledgeArticles.AddRange(article1, article2, article3);
        await dbContext.SaveChangesAsync();

        var handler = new GetSuggestedArticlesHandler(dbContext);
        var query = new GetSuggestedArticlesQuery(
            TicketTitle: null,
            TicketDescription: null,
            CategoryId: null,
            Tags: null,
            Limit: 2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Most Helpful Article");
        result[1].Title.Should().Be("Less Helpful Article");
    }

    [Fact]
    public async Task Handle_WithLimit_ReturnsNoMoreThanLimit()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        // Create 10 articles
        for (int i = 1; i <= 10; i++)
        {
            var article = TestDataBuilder.CreateTestKnowledgeArticle(
                authorId: author.Id,
                title: $"Article {i}",
                helpfulCount: i);
            dbContext.KnowledgeArticles.Add(article);
        }
        await dbContext.SaveChangesAsync();

        var handler = new GetSuggestedArticlesHandler(dbContext);
        var query = new GetSuggestedArticlesQuery(
            TicketTitle: null,
            TicketDescription: null,
            CategoryId: null,
            Tags: null,
            Limit: 3);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_OnlyReturnsPublishedArticles()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        var publishedArticle = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Published Article",
            status: ArticleStatus.Published,
            helpfulCount: 10);
        
        var draftArticle = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Draft Article",
            status: ArticleStatus.Draft,
            helpfulCount: 20);

        var archivedArticle = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Archived Article",
            status: ArticleStatus.Archived,
            helpfulCount: 15);

        dbContext.KnowledgeArticles.AddRange(publishedArticle, draftArticle, archivedArticle);
        await dbContext.SaveChangesAsync();

        var handler = new GetSuggestedArticlesHandler(dbContext);
        var query = new GetSuggestedArticlesQuery(
            TicketTitle: null,
            TicketDescription: null,
            CategoryId: null,
            Tags: null,
            Limit: 5);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Published Article");
    }
}
