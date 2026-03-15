using FluentAssertions;
using Hickory.Api.Features.KnowledgeBase.IncrementViewCount;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Tests.Features.KnowledgeBase;

public class IncrementArticleViewCountHandlerTests
{
    [Fact]
    public async Task Handle_WithValidArticle_IncrementsViewCount()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var cacheService = new MockCacheService();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        var article = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Test Article",
            viewCount: 5);
        dbContext.KnowledgeArticles.Add(article);
        await dbContext.SaveChangesAsync();

        var handler = new IncrementArticleViewCountHandler(dbContext, cacheService);
        var command = new IncrementArticleViewCountCommand(article.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedArticle = await dbContext.KnowledgeArticles
            .FirstOrDefaultAsync(a => a.Id == article.Id);
        updatedArticle.Should().NotBeNull();
        updatedArticle!.ViewCount.Should().Be(6);
    }

    [Fact]
    public async Task Handle_WithZeroViewCount_IncrementsToOne()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var cacheService = new MockCacheService();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        var article = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "New Article",
            viewCount: 0);
        dbContext.KnowledgeArticles.Add(article);
        await dbContext.SaveChangesAsync();

        var handler = new IncrementArticleViewCountHandler(dbContext, cacheService);
        var command = new IncrementArticleViewCountCommand(article.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedArticle = await dbContext.KnowledgeArticles
            .FirstOrDefaultAsync(a => a.Id == article.Id);
        updatedArticle.Should().NotBeNull();
        updatedArticle!.ViewCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNonExistentArticle_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var cacheService = new MockCacheService();

        var handler = new IncrementArticleViewCountHandler(dbContext, cacheService);
        var command = new IncrementArticleViewCountCommand(Guid.NewGuid());

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_CalledMultipleTimes_IncrementsCorrectly()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var cacheService = new MockCacheService();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        var article = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Popular Article",
            viewCount: 0);
        dbContext.KnowledgeArticles.Add(article);
        await dbContext.SaveChangesAsync();

        var handler = new IncrementArticleViewCountHandler(dbContext, cacheService);

        // Act
        await handler.Handle(new IncrementArticleViewCountCommand(article.Id), CancellationToken.None);
        await handler.Handle(new IncrementArticleViewCountCommand(article.Id), CancellationToken.None);
        await handler.Handle(new IncrementArticleViewCountCommand(article.Id), CancellationToken.None);

        // Assert
        var updatedArticle = await dbContext.KnowledgeArticles
            .FirstOrDefaultAsync(a => a.Id == article.Id);
        updatedArticle.Should().NotBeNull();
        updatedArticle!.ViewCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_DoesNotAffectOtherArticles()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var cacheService = new MockCacheService();
        var author = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(author);

        var article1 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article 1",
            viewCount: 10);
        var article2 = TestDataBuilder.CreateTestKnowledgeArticle(
            authorId: author.Id,
            title: "Article 2",
            viewCount: 20);
        dbContext.KnowledgeArticles.AddRange(article1, article2);
        await dbContext.SaveChangesAsync();

        var handler = new IncrementArticleViewCountHandler(dbContext, cacheService);

        // Act
        await handler.Handle(new IncrementArticleViewCountCommand(article1.Id), CancellationToken.None);

        // Assert
        var updatedArticle1 = await dbContext.KnowledgeArticles
            .FirstOrDefaultAsync(a => a.Id == article1.Id);
        var updatedArticle2 = await dbContext.KnowledgeArticles
            .FirstOrDefaultAsync(a => a.Id == article2.Id);

        updatedArticle1!.ViewCount.Should().Be(11);
        updatedArticle2!.ViewCount.Should().Be(20); // Unchanged
    }
}
