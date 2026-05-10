using StockScreener.Application.Interfaces;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Domain.Interfaces.Services;

namespace StockScreener.Application.Services;

public class MarketDataService(
    IStockRepository stockRepository,
    IPriceHistoryRepository priceHistoryRepository,
    IFundamentalsRepository fundamentalsRepository,
    IMarketDataProvider marketDataProvider) : IMarketDataService
{
    public async Task SyncAllAsync(CancellationToken cancellationToken = default)
    {
        var symbols = await marketDataProvider.GetAvailableSymbolsAsync(cancellationToken);

        foreach (var symbol in symbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SyncSymbolAsync(symbol, cancellationToken);
        }
    }

    public async Task SyncSymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        // Sync quote (creates or updates the Stock row)
        var quote = await marketDataProvider.GetQuoteAsync(symbol, cancellationToken);
        if (quote is null)
            return;

        var existing = await stockRepository.GetBySymbolAsync(symbol, cancellationToken);
        if (existing is null)
            await stockRepository.AddAsync(quote, cancellationToken);
        else
            await stockRepository.UpdateAsync(quote, cancellationToken);

        var stockId = existing?.Id ?? quote.Id;

        // Sync fundamentals
        var fundamentals = await marketDataProvider.GetFundamentalsAsync(symbol, cancellationToken);
        if (fundamentals is not null)
        {
            var existingFundamentals = await fundamentalsRepository.GetByStockIdAsync(stockId, cancellationToken);
            if (existingFundamentals is null)
                await fundamentalsRepository.AddAsync(fundamentals, cancellationToken);
            else
                await fundamentalsRepository.UpdateAsync(fundamentals, cancellationToken);
        }

        // Sync recent price candles (last 30 days)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = today.AddDays(-30);
        var candles = await marketDataProvider.GetCandlesAsync(symbol, from, today, cancellationToken);
        if (candles.Any())
            await priceHistoryRepository.AddRangeAsync(candles, cancellationToken);
    }
}
