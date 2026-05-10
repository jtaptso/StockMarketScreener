using StockScreener.Domain.Entities;

namespace StockScreener.Domain.Interfaces.Repositories;

public interface IPriceHistoryRepository
{
    Task<PriceHistory?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PriceHistory>> GetByStockIdAsync(int stockId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PriceHistory>> GetByStockIdAndDateRangeAsync(int stockId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<PriceHistory?> GetLatestByStockIdAsync(int stockId, CancellationToken cancellationToken = default);
    Task AddAsync(PriceHistory priceHistory, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<PriceHistory> priceHistories, CancellationToken cancellationToken = default);
    Task DeleteByStockIdAndDateAsync(int stockId, DateOnly date, CancellationToken cancellationToken = default);
}
