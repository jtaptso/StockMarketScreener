using StockScreener.Application.DTOs;

namespace StockScreener.Application.Interfaces;

public interface IStockService
{
    Task<StockDto?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PriceHistoryDto>> GetPriceHistoryAsync(
        string symbol,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<FundamentalsDto?> GetFundamentalsAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}
