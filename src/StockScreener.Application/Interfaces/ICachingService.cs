namespace StockScreener.Application.Interfaces;

/// <summary>
/// Generic cache abstraction. Implementations live in Infrastructure.
/// </summary>
public interface ICachingService
{
    /// <summary>Returns the cached value if present; otherwise invokes <paramref name="factory"/>, caches and returns the result.</summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>Returns the cached value, or <see langword="default"/> if not found.</summary>
    T? Get<T>(string key);

    /// <summary>Stores a value with an absolute expiry.</summary>
    void Set<T>(string key, T value, TimeSpan ttl);

    /// <summary>Removes a cached entry.</summary>
    void Remove(string key);
}
