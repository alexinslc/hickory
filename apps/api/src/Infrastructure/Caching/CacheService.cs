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
        try
        {
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }

            await _distributedCache.SetStringAsync(key, serialized, options, cancellationToken);
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", 
                SanitizeForLogging(key), expiration?.ToString() ?? "none");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache for key: {Key}", SanitizeForLogging(key));
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
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToArray();
            
            if (keys.Length == 0)
            {
                return;
            }

            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(keys);
            
            _logger.LogDebug("Removed {Count} cache keys matching pattern: {Pattern}", 
                keys.Length, pattern);
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
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var info = await server.InfoAsync("stats");
            
            var keyspaceInfo = await server.InfoAsync("keyspace");
            var totalKeys = 0L;
            
            // Parse keyspace info to get total keys
            foreach (var section in keyspaceInfo)
            {
                foreach (var entry in section)
                {
                    if (entry.Key.StartsWith("db"))
                    {
                        var value = entry.Value;
                        var parts = value.Split(',');
                        foreach (var part in parts)
                        {
                            if (part.StartsWith("keys="))
                            {
                                if (long.TryParse(part.Substring(5), out var keys))
                                {
                                    totalKeys += keys;
                                }
                            }
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
