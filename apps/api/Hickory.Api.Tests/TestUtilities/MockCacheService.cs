using Hickory.Api.Infrastructure.Caching;

namespace Hickory.Api.Tests.TestUtilities;

/// <summary>
/// Mock implementation of ICacheService for unit testing.
/// Does not actually cache anything - just passes through to the factory function.
/// </summary>
public class MockCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        // Always return null (cache miss) for simplicity in tests
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        // No-op for tests
        return Task.CompletedTask;
    }

    public Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Always execute factory (simulate cache miss)
        return factory(cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // No-op for tests
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // No-op for tests
        return Task.CompletedTask;
    }

    public Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        // No-op for tests
        return Task.CompletedTask;
    }

    public (long Hits, long Misses, double HitRate) GetStatistics()
    {
        return (0, 0, 0);
    }

    public void ResetStatistics()
    {
        // No-op for tests
    }
}
