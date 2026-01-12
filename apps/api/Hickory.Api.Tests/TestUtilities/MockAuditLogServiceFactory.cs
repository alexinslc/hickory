using Hickory.Api.Infrastructure.Audit;
using Hickory.Api.Infrastructure.Data.Entities;
using Moq;

namespace Hickory.Api.Tests.TestUtilities;

/// <summary>
/// Factory for creating mock IAuditLogService for tests
/// </summary>
public static class MockAuditLogServiceFactory
{
    /// <summary>
    /// Creates a mock IAuditLogService that does nothing (for tests that don't care about auditing)
    /// </summary>
    public static Mock<IAuditLogService> Create()
    {
        var mock = new Mock<IAuditLogService>();
        
        mock.Setup(x => x.LogAsync(
                It.IsAny<AuditAction>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        mock.Setup(x => x.LogAuthEventAsync(
                It.IsAny<AuditAction>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        return mock;
    }
}
