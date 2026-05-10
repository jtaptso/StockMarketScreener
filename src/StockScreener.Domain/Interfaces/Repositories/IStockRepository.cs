using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;

namespace StockScreener.Domain.Interfaces.Repositories;

public interface IStockRepository
{
    Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetByExchangeAsync(Exchange exchange, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetBySectorAsync(string sector, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string symbol, CancellationToken cancellationToken = default);
    Task AddAsync(Stock stock, CancellationToken cancellationToken = default);
    Task UpdateAsync(Stock stock, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
