using Microsoft.Extensions.Caching.Distributed;

namespace UniPharApi.Services;

public class CacheService
{
    private const string VersionKey = "content:version";
    private const string FallbackVersion = "cache-unavailable";

    // After a cache failure, skip the cache entirely for this long.
    private static readonly TimeSpan BreakerOpenFor = TimeSpan.FromSeconds(30);

    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    // UTC ticks until which the breaker is open (0 = closed).
    // CacheService is a singleton, so this is shared by all requests.
    private long _openUntilTicks;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    private bool BreakerOpen =>
        DateTime.UtcNow.Ticks < Interlocked.Read(ref _openUntilTicks);

    private void TripBreaker(Exception ex, string what)
    {
        Interlocked.Exchange(ref _openUntilTicks, DateTime.UtcNow.Add(BreakerOpenFor).Ticks);
        _logger.LogWarning(ex, "Cache {What} failed; skipping cache for {Seconds}s", what, BreakerOpenFor.TotalSeconds);
    }

    private void CloseBreaker() => Interlocked.Exchange(ref _openUntilTicks, 0);

    public async Task<string?> GetAsync(string key)
    {
        if (BreakerOpen) return null;
        try
        {
            return await _cache.GetStringAsync(key);
        }
        catch (Exception ex)
        {
            TripBreaker(ex, "read");
            return null;
        }
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (BreakerOpen) return;
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
            TripBreaker(ex, "write");
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (BreakerOpen) return;
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            TripBreaker(ex, "remove");
        }
    }

    public async Task<string> GetVersionAsync()
    {
        if (BreakerOpen) return FallbackVersion;
        try
        {
            var version = await _cache.GetStringAsync(VersionKey);
            if (version is not null)
                return version;

            return await BumpVersionAsync();
        }
        catch (Exception ex)
        {
            TripBreaker(ex, "version lookup");
            return FallbackVersion;
        }
    }

    // Deliberately strict and never skipped by the breaker: the webhook must try
    // Redis and report failure if the bump doesn't happen. A success closes the breaker.
    public async Task<string> BumpVersionAsync()
    {
        var version = DateTime.UtcNow.Ticks.ToString();
        await _cache.SetStringAsync(VersionKey, version, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        });
        CloseBreaker();
        return version;
    }
}