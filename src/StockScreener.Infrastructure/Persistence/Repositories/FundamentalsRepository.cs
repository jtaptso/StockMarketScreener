using Microsoft.EntityFrameworkCore;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Infrastructure.Persistence;

namespace StockScreener.Infrastructure.Persistence.Repositories;

public class FundamentalsRepository(AppDbContext db) : IFundamentalsRepository
{
    public async Task<Fundamentals?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.Fundamentals.FindAsync([id], cancellationToken);

    public async Task<Fundamentals?> GetByStockIdAsync(int stockId, CancellationToken cancellationToken = default)
        => await db.Fundamentals
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.StockId == stockId, cancellationToken);

    public async Task AddAsync(Fundamentals fundamentals, CancellationToken cancellationToken = default)
    {
        await db.Fundamentals.AddAsync(fundamentals, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Fundamentals fundamentals, CancellationToken cancellationToken = default)
    {
        db.Fundamentals.Update(fundamentals);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByStockIdAsync(int stockId, CancellationToken cancellationToken = default)
    {
        var row = await db.Fundamentals
            .FirstOrDefaultAsync(f => f.StockId == stockId, cancellationToken);
        if (row is not null)
        {
            db.Fundamentals.Remove(row);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
