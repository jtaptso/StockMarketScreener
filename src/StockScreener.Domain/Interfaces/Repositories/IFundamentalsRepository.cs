using StockScreener.Domain.Entities;

namespace StockScreener.Domain.Interfaces.Repositories;

public interface IFundamentalsRepository
{
    Task<Fundamentals?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Fundamentals?> GetByStockIdAsync(int stockId, CancellationToken cancellationToken = default);
    Task AddAsync(Fundamentals fundamentals, CancellationToken cancellationToken = default);
    Task UpdateAsync(Fundamentals fundamentals, CancellationToken cancellationToken = default);
    Task DeleteByStockIdAsync(int stockId, CancellationToken cancellationToken = default);
}
