using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Hickory.Api.Infrastructure.Caching;

/// <summary>
/// Redis-based distributed cache service implementation
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Meter _meter;
    private readonly Counter<long> _cacheHitsCounter;
    private readonly Counter<long> _cacheMissesCounter;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    
    private long _hits;
    private long _misses;

    private static string? SanitizeForLogging(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Remove line breaks and other control characters to prevent log forging
        var chars = value.Where(c => !char.IsControl(c) || c == '\t').ToArray();
        return new string(chars);
    }

    public CacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer redis,
        ILogger<CacheService> logger,
        IMeterFactory meterFactory)
    {
        _distributedCache = distributedCache;
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        // Initialize metrics
        _meter = meterFactory.Create("Hickory.Api.Cache");
        _cacheHitsCounter = _meter.CreateCounter<long>("cache.hits", "hits", "Number of cache hits");
        _cacheMissesCounter = _meter.CreateCounter<long>("cache.misses", "misses", "Number of cache misses");
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Try to get from cache
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // Use a semaphore to prevent cache stampede - only one factory execution per key
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check cache after acquiring lock
            cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            // Execute factory to get value
            var value = await factory(cancellationToken);
            if (value == null)
            {
                return null;
            }

            // Cache the result
            await SetAsync(key, value, expiration, cancellationToken);
            
            return value;
        }
        finally
        {
            semaphore.Release();
            
            // Opportunistically remove the semaphore from the dictionary to avoid
            // unbounded growth for keys that are no longer used. We only remove
            // when the semaphore is not contended (CurrentCount == 1) and the
            // instance in the dictionary is the same one we used.
            if (semaphore.CurrentCount == 1 &&
                _locks.TryRemove(key, out var existingSemaphore) &&
                !ReferenceEquals(existingSemaphore, semaphore))
            {
                // If a different semaphore was removed (extremely unlikely due to the
                // use of GetOrAdd), reinsert it to avoid affecting other callers.
                _locks.TryAdd(key, existingSemaphore);
            }
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key, cancellationToken);
            
            if (cachedData == null)
            {
                Interlocked.Increment(ref _misses);
                _cacheMissesCounter.Add(1);
                _logger.LogDebug("Cache miss for key: {Key}", SanitizeForLogging(key));
                return null;
            }

            Interlocked.Increment(ref _hits);
            _cacheHitsCounter.Add(1);
            _logger.LogDebug("Cache hit for key: {Key}", SanitizeForLogging(key));
            
            return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
        }
        catch (Exception ex)
        {
            _cacheMissesCounter.Add(1);
            _logger.LogWarning(ex, "Error retrieving from cache for key: {Key}", SanitizeForLogging(key));
            Interlocked.Increment(ref _misses);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Best-effort cache update with bounded retries and exponential backoff
        try
        {
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);

            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }

            const int maxRetries = 3;
            var delayMilliseconds = 100;

            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await _distributedCache.SetStringAsync(key, serialized, options, cancellationToken);
                    _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", 
                        SanitizeForLogging(key), expiration?.ToString() ?? "none");
                    return;
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    if (attempt >= maxRetries)
                    {
                        _logger.LogError(ex,
                            "Failed to set cache for key: {Key} after {Attempts} attempts. " +
                            "Cache entry may remain stale or missing.",
                            SanitizeForLogging(key), attempt);
                        return;
                    }

                    _logger.LogWarning(ex,
                        "Error setting cache for key: {Key} on attempt {Attempt} of {MaxAttempts}. " +
                        "Retrying in {DelayMilliseconds}ms.",
                        SanitizeForLogging(key), attempt, maxRetries, delayMilliseconds);

                    try
                    {
                        await Task.Delay(delayMilliseconds, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Respect cancellation and exit without throwing to callers
                        return;
                    }

                    delayMilliseconds *= 2;
                }
            }
        }
        catch (Exception ex)
        {
            // Serialization or configuration errors are logged once and not retried
            _logger.LogWarning(ex, "Error preparing cache value for key: {Key}", SanitizeForLogging(key));
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Removed cache for key: {Key}", SanitizeForLogging(key));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache for key: {Key}", SanitizeForLogging(key));
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();

            long cursor = 0;
            const int pageSize = 500;
            long totalDeleted = 0;

            do
            {
                var scanResultRaw = await db
                    .ExecuteAsync("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", pageSize.ToString())
                    .ConfigureAwait(false);
                    
                if (scanResultRaw.IsNull)
                {
                    break;
                }

                var scanResult = (RedisResult[])scanResultRaw!;
                if (scanResult.Length < 2)
                {
                    break;
                }

                cursor = (long)scanResult[0];
                var keysResultRaw = scanResult[1];
                
                if (keysResultRaw.IsNull)
                {
                    continue;
                }
                
                var keysResult = (RedisResult[])keysResultRaw!;

                if (keysResult.Length == 0)
                {
                    continue;
                }

                // Filter out null or empty keys
                var keys = new List<RedisKey>();
                for (int i = 0; i < keysResult.Length; i++)
                {
                    var keyString = (string?)keysResult[i];
                    if (!string.IsNullOrEmpty(keyString))
                    {
                        keys.Add(keyString);
                    }
                }

                if (keys.Count > 0)
                {
                    var deletedCount = await db.KeyDeleteAsync(keys.ToArray()).ConfigureAwait(false);
                    totalDeleted += deletedCount;
                }
            }
            while (cursor != 0);

            if (totalDeleted > 0)
            {
                _logger.LogDebug("Removed {Count} cache keys matching pattern: {Pattern}",
                    totalDeleted, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache by pattern: {Pattern}", pattern);
        }
    }

    public async Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var endPoints = _redis.GetEndPoints();
            if (endPoints == null || endPoints.Length == 0)
            {
                throw new InvalidOperationException("No Redis endpoints are configured.");
            }

            IServer? server = null;
            foreach (var endPoint in endPoints)
            {
                var candidate = _redis.GetServer(endPoint);
                if (candidate.IsConnected && !candidate.IsReplica)
                {
                    server = candidate;
                    break;
                }
            }

            // Fallback to the first endpoint if no suitable primary/non-replica server was found
            server ??= _redis.GetServer(endPoints.First());
            
            var keyspaceInfo = await server.InfoAsync("keyspace");
            var totalKeys = 0L;
            
            // Parse keyspace info to get total keys
            foreach (var section in keyspaceInfo)
            {
                var dbEntries = section.Where(e => e.Key.StartsWith("db"));
                foreach (var entry in dbEntries)
                {
                    var parts = entry.Value.Split(',');
                    var keysParts = parts.Where(p => p.StartsWith("keys=")).Select(p => p["keys=".Length..]);
                    foreach (var keyPart in keysParts)
                    {
                        if (long.TryParse(keyPart, out var keys))
                        {
                            totalKeys += keys;
                        }
                    }
                }
            }

            return new CacheStatistics
            {
                TotalHits = Interlocked.Read(ref _hits),
                TotalMisses = Interlocked.Read(ref _misses),
                TotalKeys = totalKeys
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting cache statistics");
            return new CacheStatistics
            {
                TotalHits = Interlocked.Read(ref _hits),
                TotalMisses = Interlocked.Read(ref _misses),
                TotalKeys = 0
            };
        }
    }
}
