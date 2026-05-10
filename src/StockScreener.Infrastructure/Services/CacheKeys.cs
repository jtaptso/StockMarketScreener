namespace StockScreener.Infrastructure.Services;

/// <summary>
/// Central registry of cache key templates and TTL constants.
/// </summary>
public static class CacheKeys
{
    // ── Stocks ────────────────────────────────────────────────────────────────
    /// <summary>Single stock by symbol. TTL: 30 seconds (quote data).</summary>
    public static string Stock(string symbol) => $"stock:{symbol.ToUpperInvariant()}";
    public static readonly TimeSpan StockTtl = TimeSpan.FromSeconds(30);

    /// <summary>All stocks (full list for screener). TTL: 10 seconds.</summary>
    public const string AllStocks = "stocks:all";
    public static readonly TimeSpan AllStocksTtl = TimeSpan.FromSeconds(10);

    // ── Screener Results ──────────────────────────────────────────────────────
    /// <summary>Screener result page keyed on the serialized filter. TTL: 10 seconds.</summary>
    public static string ScreenerResult(string filterHash) => $"screener:{filterHash}";
    public static readonly TimeSpan ScreenerResultTtl = TimeSpan.FromSeconds(10);

    // ── Price History ─────────────────────────────────────────────────────────
    /// <summary>Price history for a stock over a date range. TTL: 1 hour.</summary>
    public static string PriceHistory(string symbol, string from, string to)
        => $"pricehistory:{symbol.ToUpperInvariant()}:{from}:{to}";
    public static readonly TimeSpan PriceHistoryTtl = TimeSpan.FromHours(1);

    // ── Fundamentals ──────────────────────────────────────────────────────────
    /// <summary>Fundamentals for a stock. TTL: 24 hours.</summary>
    public static string Fundamentals(string symbol) => $"fundamentals:{symbol.ToUpperInvariant()}";
    public static readonly TimeSpan FundamentalsTtl = TimeSpan.FromHours(24);

    // ── Watchlists ────────────────────────────────────────────────────────────
    /// <summary>All watchlists for a user. TTL: 5 minutes.</summary>
    public static string UserWatchlists(string userId) => $"watchlists:{userId}";
    public static readonly TimeSpan UserWatchlistsTtl = TimeSpan.FromMinutes(5);

    // ── Filter Presets ────────────────────────────────────────────────────────
    /// <summary>Saved filter presets for a user. TTL: 5 minutes.</summary>
    public static string UserPresets(string userId) => $"presets:{userId}";
    public static readonly TimeSpan UserPresetsTtl = TimeSpan.FromMinutes(5);

    // ── Available Symbols ─────────────────────────────────────────────────────
    /// <summary>Provider symbol list. TTL: 24 hours.</summary>
    public const string AvailableSymbols = "symbols:available";
    public static readonly TimeSpan AvailableSymbolsTtl = TimeSpan.FromHours(24);
}
