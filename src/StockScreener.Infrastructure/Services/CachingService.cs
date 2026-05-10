using Microsoft.Extensions.Caching.Memory;
using StockScreener.Application.Interfaces;

namespace StockScreener.Infrastructure.Services;

/// <summary>
/// <see cref="ICachingService"/> implementation backed by <see cref="IMemoryCache"/>.
/// </summary>
public class CachingService(IMemoryCache cache) : ICachingService
{
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        var value = await factory();
        Set(key, value, ttl);
        return value;
    }

    public T? Get<T>(string key)
    {
        cache.TryGetValue(key, out T? value);
        return value;
    }

    public void Set<T>(string key, T value, TimeSpan ttl)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };
        cache.Set(key, value, options);
    }

    public void Remove(string key) => cache.Remove(key);
}
