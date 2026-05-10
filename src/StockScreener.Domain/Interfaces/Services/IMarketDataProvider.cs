using StockScreener.Domain.Entities;

namespace StockScreener.Domain.Interfaces.Services;

/// <summary>
/// Port (anti-corruption layer) for external market data sources
/// (e.g. Finnhub, AlphaVantage). Implementations live in Infrastructure.
/// </summary>
public interface IMarketDataProvider
{
    /// <summary>Fetches the current quote for a single symbol.</summary>
    Task<Stock?> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>Fetches OHLCV candle data for a symbol over the given date range.</summary>
    Task<IEnumerable<PriceHistory>> GetCandlesAsync(
        string symbol,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    /// <summary>Fetches the latest fundamental data for a symbol.</summary>
    Task<Fundamentals?> GetFundamentalsAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>Returns all symbols available on the provider.</summary>
    Task<IEnumerable<string>> GetAvailableSymbolsAsync(CancellationToken cancellationToken = default);
}
