using StockScreener.Domain.Enums;

namespace StockScreener.Domain.ValueObjects;

/// <summary>
/// Immutable set of criteria applied by <c>IScreenerEngine</c>.
/// Null ranges mean "no constraint" for that field.
/// All active criteria are AND-ed together.
/// </summary>
public sealed record ScreenerFilter
{
    // ── Price ───────────────────────────────────────────────
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }

    // ── Market Cap ──────────────────────────────────────────
    public decimal? MinMarketCap { get; init; }
    public decimal? MaxMarketCap { get; init; }
    public IReadOnlyList<MarketCapCategory>? MarketCapCategories { get; init; }

    // ── Exchange / Sector ────────────────────────────────────
    public IReadOnlyList<Exchange>? Exchanges { get; init; }
    public IReadOnlyList<string>? Sectors { get; init; }

    // ── Volume ───────────────────────────────────────────────
    public long? MinVolume { get; init; }
    public long? MaxVolume { get; init; }

    // ── Valuation ────────────────────────────────────────────
    public decimal? MinPE { get; init; }
    public decimal? MaxPE { get; init; }
    public decimal? MinPB { get; init; }
    public decimal? MaxPB { get; init; }
    public decimal? MinDividendYield { get; init; }
    public decimal? MaxDividendYield { get; init; }

    // ── Technical ────────────────────────────────────────────
    public decimal? MinRSI { get; init; }
    public decimal? MaxRSI { get; init; }
    public decimal? MinBeta { get; init; }
    public decimal? MaxBeta { get; init; }

    // ── Sorting / Paging ─────────────────────────────────────
    public string? SortBy { get; init; }
    public SortOrder SortOrder { get; init; } = SortOrder.Ascending;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
