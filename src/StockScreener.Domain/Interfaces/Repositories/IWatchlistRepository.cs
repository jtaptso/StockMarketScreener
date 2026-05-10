using StockScreener.Domain.Entities;

namespace StockScreener.Domain.Interfaces.Repositories;

public interface IWatchlistRepository
{
    Task<Watchlist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Watchlist>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(Watchlist watchlist, CancellationToken cancellationToken = default);
    Task UpdateAsync(Watchlist watchlist, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddItemAsync(WatchlistItem item, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid watchlistId, int stockId, CancellationToken cancellationToken = default);
    Task<bool> ItemExistsAsync(Guid watchlistId, int stockId, CancellationToken cancellationToken = default);
}
