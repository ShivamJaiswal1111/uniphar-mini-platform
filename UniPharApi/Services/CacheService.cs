using Microsoft.Extensions.Caching.Distributed;

namespace UniPharApi.Services;

public class CacheService
{
    private const string VersionKey = "content:version";

    // Used as the content version while the cache is unreachable. Nothing can be
    // read or written then anyway, so the exact value doesn't matter.
    private const string FallbackVersion = "cache-unavailable";

    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    // Fail open: a cache error is treated as a cache miss.
    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await _cache.GetStringAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for {Key}; treating as miss", key);
            return null;
        }
    }

    // Fail open: if the write fails, the response is still returned, just not cached.
    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(key, value, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for {Key}; continuing without caching", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache remove failed for {Key}", key);
        }
    }

    // Current content version. If the version key is missing (first run, Redis flushed,
    // or it expired), create a fresh one. A new version can only cause cache misses,
    // never stale reads, so this is always safe. If the cache is down, return a fixed
    // fallback so content requests still go through to Umbraco.
    public async Task<string> GetVersionAsync()
    {
        try
        {
            var version = await _cache.GetStringAsync(VersionKey);
            if (version is not null)
                return version;

            return await BumpVersionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache version lookup failed; using fallback version");
            return FallbackVersion;
        }
    }

    // Called by the webhook: every existing content key becomes unreachable at once.
    // Deliberately strict (no try/catch): if the bump fails, the webhook must report
    // failure so a stale cache doesn't go unnoticed.
    public async Task<string> BumpVersionAsync()
    {
        var version = DateTime.UtcNow.Ticks.ToString();
        await _cache.SetStringAsync(VersionKey, version, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        });
        return version;
    }
}