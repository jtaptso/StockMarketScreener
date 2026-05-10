using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Domain.Interfaces.Services;
using StockScreener.Infrastructure.External.AlphaVantage;

namespace StockScreener.Infrastructure.External;

/// <summary>
/// Fallback <see cref="IMarketDataProvider"/> using the AlphaVantage REST API.
/// Registered as a typed <see cref="HttpClient"/> in DI.
/// </summary>
public class AlphaVantageClient(HttpClient http, ILogger<AlphaVantageClient> logger) : IMarketDataProvider
{
    // ── IMarketDataProvider ───────────────────────────────────────────────────

    public async Task<Stock?> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await http.GetFromJsonAsync<AlphaVantageQuoteResponse>(
                $"query?function=GLOBAL_QUOTE&symbol={symbol}", cancellationToken);

            var q = response?.GlobalQuote;
            if (q is null || string.IsNullOrEmpty(q.Price))
                return null;

            return new Stock
            {
                Symbol       = symbol.ToUpperInvariant(),
                CompanyName  = symbol,   // AlphaVantage GLOBAL_QUOTE does not return company name
                Exchange     = Exchange.NYSE,
                CurrentPrice = ParseDecimal(q.Price),
                DayHigh      = ParseDecimal(q.High),
                DayLow       = ParseDecimal(q.Low),
                Volume       = ParseLong(q.Volume),
                LastUpdated  = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AlphaVantage GetQuoteAsync failed for symbol {Symbol}", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<PriceHistory>> GetCandlesAsync(
        string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use compact output (last 100 trading days); full outputsize costs extra API calls.
            var response = await http.GetFromJsonAsync<AlphaVantageDailyResponse>(
                $"query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=compact",
                cancellationToken);

            if (response?.TimeSeries is null)
                return [];

            var candles = new List<PriceHistory>();
            foreach (var (dateStr, bar) in response.TimeSeries)
            {
                if (!DateOnly.TryParse(dateStr, out var date)) continue;
                if (date < from || date > to) continue;

                candles.Add(new PriceHistory
                {
                    TradeDate  = date,
                    OpenPrice  = ParseDecimal(bar.Open),
                    HighPrice  = ParseDecimal(bar.High),
                    LowPrice   = ParseDecimal(bar.Low),
                    ClosePrice = ParseDecimal(bar.Close),
                    Volume     = ParseLong(bar.Volume)
                });
            }

            return candles.OrderBy(c => c.TradeDate);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AlphaVantage GetCandlesAsync failed for symbol {Symbol}", symbol);
            return [];
        }
    }

    /// <summary>
    /// AlphaVantage does not expose a fundamentals endpoint on the free tier.
    /// Returns <see langword="null"/> — the sync service will simply skip fundamentals for this provider.
    /// </summary>
    public Task<Fundamentals?> GetFundamentalsAsync(string symbol, CancellationToken cancellationToken = default)
        => Task.FromResult<Fundamentals?>(null);

    /// <summary>
    /// AlphaVantage does not expose a symbol-listing endpoint on the free tier.
    /// Returns an empty list — callers should fall back to Finnhub for discovery.
    /// </summary>
    public Task<IEnumerable<string>> GetAvailableSymbolsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<string>>([]);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static decimal ParseDecimal(string value)
        => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result : 0m;

    private static long ParseLong(string value)
        => long.TryParse(value, out var result) ? result : 0L;
}
