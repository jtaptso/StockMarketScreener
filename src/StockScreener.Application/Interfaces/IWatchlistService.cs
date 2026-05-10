using StockScreener.Application.DTOs;

namespace StockScreener.Application.Interfaces;

public interface IWatchlistService
{
    Task<IEnumerable<WatchlistDto>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<WatchlistDto?> GetByIdAsync(
        Guid watchlistId,
        CancellationToken cancellationToken = default);

    Task<WatchlistDto> CreateAsync(
        string userId,
        string name,
        string? description,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid watchlistId,
        string name,
        string? description,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid watchlistId, CancellationToken cancellationToken = default);

    Task AddStockAsync(
        Guid watchlistId,
        string symbol,
        decimal? sharesOwned = null,
        decimal? costBasis = null,
        CancellationToken cancellationToken = default);

    Task RemoveStockAsync(
        Guid watchlistId,
        string symbol,
        CancellationToken cancellationToken = default);
}
