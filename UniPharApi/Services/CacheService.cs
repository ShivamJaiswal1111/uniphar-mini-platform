using Microsoft.Extensions.Caching.Distributed;

namespace UniPharApi.Services;

public class CacheService
{
    private const string VersionKey = "content:version";
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string?> GetAsync(string key)
    {
        return await _cache.GetStringAsync(key);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
        };
        await _cache.SetStringAsync(key, value, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    // Current content version. If the version key is missing (first run, Redis flushed,
    // or it expired), create a fresh one. A new version can only cause cache misses,
    // never stale reads, so this is always safe.
    public async Task<string> GetVersionAsync()
    {
        var version = await _cache.GetStringAsync(VersionKey);
        if (version is not null)
            return version;

        return await BumpVersionAsync();
    }

    // Called by the webhook: every existing content key becomes unreachable at once.
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