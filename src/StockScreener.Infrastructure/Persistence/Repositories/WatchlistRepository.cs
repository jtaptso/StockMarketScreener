using Microsoft.EntityFrameworkCore;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Infrastructure.Persistence;

namespace StockScreener.Infrastructure.Persistence.Repositories;

public class WatchlistRepository(AppDbContext db) : IWatchlistRepository
{
    public async Task<Watchlist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await db.Watchlists
            .Include(w => w.Items)
                .ThenInclude(i => i.Stock)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IEnumerable<Watchlist>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await db.Watchlists
            .AsNoTracking()
            .Include(w => w.Items)
                .ThenInclude(i => i.Stock)
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Watchlist watchlist, CancellationToken cancellationToken = default)
    {
        await db.Watchlists.AddAsync(watchlist, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Watchlist watchlist, CancellationToken cancellationToken = default)
    {
        db.Watchlists.Update(watchlist);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var watchlist = await db.Watchlists.FindAsync([id], cancellationToken);
        if (watchlist is not null)
        {
            db.Watchlists.Remove(watchlist);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddItemAsync(WatchlistItem item, CancellationToken cancellationToken = default)
    {
        await db.WatchlistItems.AddAsync(item, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(Guid watchlistId, int stockId, CancellationToken cancellationToken = default)
    {
        var item = await db.WatchlistItems
            .FirstOrDefaultAsync(i => i.WatchlistId == watchlistId && i.StockId == stockId, cancellationToken);
        if (item is not null)
        {
            db.WatchlistItems.Remove(item);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ItemExistsAsync(Guid watchlistId, int stockId, CancellationToken cancellationToken = default)
        => await db.WatchlistItems
            .AnyAsync(i => i.WatchlistId == watchlistId && i.StockId == stockId, cancellationToken);
}
