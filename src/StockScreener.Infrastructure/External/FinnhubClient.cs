using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Domain.Interfaces.Services;
using StockScreener.Infrastructure.External.Finnhub;

namespace StockScreener.Infrastructure.External;

/// <summary>
/// Implements <see cref="IMarketDataProvider"/> using the Finnhub REST API.
/// Registered as a typed <see cref="HttpClient"/> in DI.
/// </summary>
public class FinnhubClient(HttpClient http, ILogger<FinnhubClient> logger) : IMarketDataProvider
{
    // ── IMarketDataProvider ───────────────────────────────────────────────────

    public async Task<Stock?> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await http.GetFromJsonAsync<FinnhubProfileResponse>(
                $"stock/profile2?symbol={symbol}", cancellationToken);

            var quote = await http.GetFromJsonAsync<FinnhubQuoteResponse>(
                $"quote?symbol={symbol}", cancellationToken);

            if (quote is null || quote.C == 0)
                return null;

            var exchange = ParseExchange(profile?.Exchange);

            return new Stock
            {
                Symbol = symbol.ToUpperInvariant(),
                CompanyName = profile?.Name ?? symbol,
                Exchange = exchange,
                Industry = profile?.FinnhubIndustry,
                MarketCap = profile is not null && profile.MarketCapitalization > 0
                    ? profile.MarketCapitalization * 1_000_000m   // Finnhub returns value in millions
                    : null,
                CurrentPrice = quote.C,
                DayHigh = quote.H,
                DayLow = quote.L,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Finnhub GetQuoteAsync failed for symbol {Symbol}", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<PriceHistory>> GetCandlesAsync(
        string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        try
        {
            long fromUnix = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeSeconds();
            long toUnix   = new DateTimeOffset(to.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeSeconds();

            var response = await http.GetFromJsonAsync<FinnhubCandleResponse>(
                $"stock/candle?symbol={symbol}&resolution=D&from={fromUnix}&to={toUnix}",
                cancellationToken);

            if (response is null || response.S != "ok" || response.T.Count == 0)
                return [];

            var candles = new List<PriceHistory>(response.T.Count);
            for (int i = 0; i < response.T.Count; i++)
            {
                candles.Add(new PriceHistory
                {
                    TradeDate  = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(response.T[i]).UtcDateTime),
                    OpenPrice  = response.O[i],
                    HighPrice  = response.H[i],
                    LowPrice   = response.L[i],
                    ClosePrice = response.C[i],
                    Volume     = response.V[i]
                });
            }
            return candles;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Finnhub GetCandlesAsync failed for symbol {Symbol}", symbol);
            return [];
        }
    }

    public async Task<Fundamentals?> GetFundamentalsAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await http.GetFromJsonAsync<FinnhubMetricResponse>(
                $"stock/metric?symbol={symbol}&metric=all", cancellationToken);

            var m = response?.Metric;
            if (m is null)
                return null;

            return new Fundamentals
            {
                PE_Ratio       = m.PeRatio,
                PB_Ratio       = m.PbRatio,
                PS_Ratio       = m.PsRatio,
                EPS            = m.Eps,
                DividendYield  = m.DividendYield,
                DebtToEquity   = m.DebtToEquity,
                CurrentRatio   = m.CurrentRatio,
                ROE            = m.Roe,
                ProfitMargin   = m.ProfitMargin,
                OperatingMargin = m.OperatingMargin,
                GrossMargin    = m.GrossMargin,
                RevenueGrowth  = m.RevenueGrowth,
                EPSGrowth      = m.EpsGrowth,
                Revenue        = m.Revenue,
                NetIncome      = m.NetIncome,
                LastUpdated    = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Finnhub GetFundamentalsAsync failed for symbol {Symbol}", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<string>> GetAvailableSymbolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var symbols = await http.GetFromJsonAsync<IReadOnlyList<FinnhubSymbolResponse>>(
                "stock/symbol?exchange=US", cancellationToken);

            return symbols?
                .Where(s => s.Type == "Common Stock")
                .Select(s => s.Symbol)
                ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Finnhub GetAvailableSymbolsAsync failed");
            return [];
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Exchange ParseExchange(string? exchangeName) => exchangeName?.ToUpperInvariant() switch
    {
        "NEW YORK STOCK EXCHANGE" or "NYSE" => Exchange.NYSE,
        "NASDAQ"                            => Exchange.NASDAQ,
        "NYSE MKT" or "AMEX"               => Exchange.AMEX,
        _                                   => Exchange.OTC
    };
}
