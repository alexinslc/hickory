using FluentAssertions;
using Hickory.Api.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;

namespace Hickory.Api.Tests.Infrastructure.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoHeaderProvided_GeneratesCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        string? capturedCorrelationId = null;

        var middleware = new CorrelationIdMiddleware(next: (innerContext) =>
        {
            capturedCorrelationId = innerContext.Items["CorrelationId"]?.ToString();
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        capturedCorrelationId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(capturedCorrelationId, out _).Should().BeTrue("generated correlation ID should be a valid GUID");
    }

    [Fact]
    public async Task InvokeAsync_HeaderProvided_UsesExistingCorrelationId()
    {
        // Arrange
        var existingId = "my-custom-correlation-id";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = existingId;
        string? capturedCorrelationId = null;

        var middleware = new CorrelationIdMiddleware(next: (innerContext) =>
        {
            capturedCorrelationId = innerContext.Items["CorrelationId"]?.ToString();
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        capturedCorrelationId.Should().Be(existingId);
    }

    [Fact]
    public async Task InvokeAsync_EmptyHeader_GeneratesNewCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "";
        string? capturedCorrelationId = null;

        var middleware = new CorrelationIdMiddleware(next: (innerContext) =>
        {
            capturedCorrelationId = innerContext.Items["CorrelationId"]?.ToString();
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        capturedCorrelationId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(capturedCorrelationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_StoresCorrelationIdInHttpContextItems()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var middleware = new CorrelationIdMiddleware(next: _ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().NotBeNull();
        context.Items["CorrelationId"].Should().BeOfType<string>();
        Guid.TryParse(context.Items["CorrelationId"]!.ToString(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_CorrelationIdAvailableDuringNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var providedId = "test-correlation-123";
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = providedId;
        string? idDuringNext = null;

        var middleware = new CorrelationIdMiddleware(next: (innerContext) =>
        {
            idDuringNext = innerContext.Items["CorrelationId"]?.ToString();
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        idDuringNext.Should().Be(providedId);
    }
}
