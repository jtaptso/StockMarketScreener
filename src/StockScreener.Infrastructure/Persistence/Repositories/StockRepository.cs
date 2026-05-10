using Microsoft.EntityFrameworkCore;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Infrastructure.Persistence;

namespace StockScreener.Infrastructure.Persistence.Repositories;

public class StockRepository(AppDbContext db) : IStockRepository
{
    public async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.Stocks.FindAsync([id], cancellationToken);

    public async Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
        => await db.Stocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Symbol == symbol.ToUpperInvariant(), cancellationToken);

    public async Task<IEnumerable<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.Stocks
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Stock>> GetByExchangeAsync(Exchange exchange, CancellationToken cancellationToken = default)
        => await db.Stocks
            .AsNoTracking()
            .Where(s => s.Exchange == exchange)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Stock>> GetBySectorAsync(string sector, CancellationToken cancellationToken = default)
        => await db.Stocks
            .AsNoTracking()
            .Where(s => s.Sector == sector)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(string symbol, CancellationToken cancellationToken = default)
        => await db.Stocks.AnyAsync(s => s.Symbol == symbol.ToUpperInvariant(), cancellationToken);

    public async Task AddAsync(Stock stock, CancellationToken cancellationToken = default)
    {
        await db.Stocks.AddAsync(stock, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Stock stock, CancellationToken cancellationToken = default)
    {
        db.Stocks.Update(stock);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var stock = await db.Stocks.FindAsync([id], cancellationToken);
        if (stock is not null)
        {
            db.Stocks.Remove(stock);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
