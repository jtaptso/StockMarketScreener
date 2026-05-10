using StockScreener.Application.DTOs;
using StockScreener.Application.Interfaces;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Interfaces.Repositories;

namespace StockScreener.Application.Services;

public class WatchlistService(
    IWatchlistRepository watchlistRepository,
    IStockRepository stockRepository) : IWatchlistService
{
    public async Task<IEnumerable<WatchlistDto>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var watchlists = await watchlistRepository.GetByUserIdAsync(userId, cancellationToken);
        return watchlists.Select(MapToDto);
    }

    public async Task<WatchlistDto?> GetByIdAsync(
        Guid watchlistId,
        CancellationToken cancellationToken = default)
    {
        var watchlist = await watchlistRepository.GetByIdAsync(watchlistId, cancellationToken);
        return watchlist is null ? null : MapToDto(watchlist);
    }

    public async Task<WatchlistDto> CreateAsync(
        string userId,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var watchlist = new Watchlist
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await watchlistRepository.AddAsync(watchlist, cancellationToken);
        return MapToDto(watchlist);
    }

    public async Task UpdateAsync(
        Guid watchlistId,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var watchlist = await watchlistRepository.GetByIdAsync(watchlistId, cancellationToken)
            ?? throw new InvalidOperationException($"Watchlist {watchlistId} not found.");

        watchlist.Name = name;
        watchlist.Description = description;
        watchlist.UpdatedAt = DateTime.UtcNow;

        await watchlistRepository.UpdateAsync(watchlist, cancellationToken);
    }

    public Task DeleteAsync(Guid watchlistId, CancellationToken cancellationToken = default)
        => watchlistRepository.DeleteAsync(watchlistId, cancellationToken);

    public async Task AddStockAsync(
        Guid watchlistId,
        string symbol,
        decimal? sharesOwned = null,
        decimal? costBasis = null,
        CancellationToken cancellationToken = default)
    {
        var stock = await stockRepository.GetBySymbolAsync(symbol, cancellationToken)
            ?? throw new InvalidOperationException($"Stock '{symbol}' not found.");

        var alreadyExists = await watchlistRepository.ItemExistsAsync(watchlistId, stock.Id, cancellationToken);
        if (alreadyExists)
            return;

        var item = new WatchlistItem
        {
            WatchlistId = watchlistId,
            StockId = stock.Id,
            SharesOwned = sharesOwned,
            CostBasis = costBasis,
            AddedAt = DateTime.UtcNow
        };

        await watchlistRepository.AddItemAsync(item, cancellationToken);
    }

    public async Task RemoveStockAsync(
        Guid watchlistId,
        string symbol,
        CancellationToken cancellationToken = default)
    {
        var stock = await stockRepository.GetBySymbolAsync(symbol, cancellationToken)
            ?? throw new InvalidOperationException($"Stock '{symbol}' not found.");

        await watchlistRepository.RemoveItemAsync(watchlistId, stock.Id, cancellationToken);
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static WatchlistDto MapToDto(Watchlist w) => new(
        w.Id, w.Name, w.Description, w.UserId,
        w.CreatedAt, w.UpdatedAt,
        w.Items.Select(MapItemToDto).ToList());

    private static WatchlistItemDto MapItemToDto(WatchlistItem i) => new(
        i.Id,
        i.StockId,
        i.Stock?.Symbol ?? string.Empty,
        i.Stock?.CompanyName ?? string.Empty,
        i.Stock?.CurrentPrice,
        i.SharesOwned,
        i.CostBasis,
        i.AddedAt);
}
