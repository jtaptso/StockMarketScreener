using Microsoft.EntityFrameworkCore;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Infrastructure.Persistence;

namespace StockScreener.Infrastructure.Persistence.Repositories;

public class PriceHistoryRepository(AppDbContext db) : IPriceHistoryRepository
{
    public async Task<PriceHistory?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await db.PriceHistory.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<PriceHistory>> GetByStockIdAsync(int stockId, CancellationToken cancellationToken = default)
        => await db.PriceHistory
            .AsNoTracking()
            .Where(p => p.StockId == stockId)
            .OrderByDescending(p => p.TradeDate)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<PriceHistory>> GetByStockIdAndDateRangeAsync(
        int stockId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => await db.PriceHistory
            .AsNoTracking()
            .Where(p => p.StockId == stockId && p.TradeDate >= from && p.TradeDate <= to)
            .OrderBy(p => p.TradeDate)
            .ToListAsync(cancellationToken);

    public async Task<PriceHistory?> GetLatestByStockIdAsync(int stockId, CancellationToken cancellationToken = default)
        => await db.PriceHistory
            .AsNoTracking()
            .Where(p => p.StockId == stockId)
            .OrderByDescending(p => p.TradeDate)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(PriceHistory priceHistory, CancellationToken cancellationToken = default)
    {
        await db.PriceHistory.AddAsync(priceHistory, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<PriceHistory> priceHistories, CancellationToken cancellationToken = default)
    {
        await db.PriceHistory.AddRangeAsync(priceHistories, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByStockIdAndDateAsync(int stockId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var row = await db.PriceHistory
            .FirstOrDefaultAsync(p => p.StockId == stockId && p.TradeDate == date, cancellationToken);
        if (row is not null)
        {
            db.PriceHistory.Remove(row);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
