using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Domain.Interfaces.Services;
using StockScreener.Domain.ValueObjects;

namespace StockScreener.Domain.Services;

/// <summary>
/// Stateless in-memory screener. Applies all non-null criteria in AND fashion,
/// then sorts and paginates the result.
/// </summary>
public class ScreenerEngine : IScreenerEngine
{
    public IEnumerable<Stock> Filter(IEnumerable<Stock> stocks, ScreenerFilter filter)
    {
        IEnumerable<Stock> result = stocks;

        // ── Price ─────────────────────────────────────────────────────────────
        if (filter.MinPrice.HasValue)
            result = result.Where(s => s.CurrentPrice >= filter.MinPrice);
        if (filter.MaxPrice.HasValue)
            result = result.Where(s => s.CurrentPrice <= filter.MaxPrice);

        // ── Market Cap ────────────────────────────────────────────────────────
        if (filter.MinMarketCap.HasValue)
            result = result.Where(s => s.MarketCap >= filter.MinMarketCap);
        if (filter.MaxMarketCap.HasValue)
            result = result.Where(s => s.MarketCap <= filter.MaxMarketCap);
        if (filter.MarketCapCategories?.Count > 0)
            result = result.Where(s => s.MarketCap.HasValue &&
                                       filter.MarketCapCategories.Contains(s.MarketCapCategory));

        // ── Exchange ─────────────────────────────────────────────────────────
        if (filter.Exchanges?.Count > 0)
            result = result.Where(s => filter.Exchanges.Contains(s.Exchange));

        // ── Sector ────────────────────────────────────────────────────────────
        if (filter.Sectors?.Count > 0)
            result = result.Where(s => s.Sector != null && filter.Sectors.Contains(s.Sector));

        // ── Volume ────────────────────────────────────────────────────────────
        if (filter.MinVolume.HasValue)
            result = result.Where(s => s.Volume >= filter.MinVolume);
        if (filter.MaxVolume.HasValue)
            result = result.Where(s => s.Volume <= filter.MaxVolume);

        // ── Fundamentals ─────────────────────────────────────────────────────
        if (filter.MinPE.HasValue)
            result = result.Where(s => s.Fundamentals != null && s.Fundamentals.PE_Ratio >= filter.MinPE);
        if (filter.MaxPE.HasValue)
            result = result.Where(s => s.Fundamentals != null && s.Fundamentals.PE_Ratio <= filter.MaxPE);

        if (filter.MinPB.HasValue)
            result = result.Where(s => s.Fundamentals != null && s.Fundamentals.PB_Ratio >= filter.MinPB);
        if (filter.MaxPB.HasValue)
            result = result.Where(s => s.Fundamentals != null && s.Fundamentals.PB_Ratio <= filter.MaxPB);

        if (filter.MinDividendYield.HasValue)
            result = result.Where(s => s.Fundamentals != null && s.Fundamentals.DividendYield >= filter.MinDividendYield);
        if (filter.MaxDividendYield.HasValue)
            result = result.Where(s => s.Fundamentals != null && s.Fundamentals.DividendYield <= filter.MaxDividendYield);

        // ── Technical: RSI ────────────────────────────────────────────────────
        if (filter.MinRSI.HasValue || filter.MaxRSI.HasValue)
        {
            result = result.Where(s =>
            {
                decimal? rsi = s.TechnicalIndicators
                    .OrderByDescending(t => t.TradeDate)
                    .FirstOrDefault()?.RSI_14;

                if (rsi is null) return false;
                if (filter.MinRSI.HasValue && rsi < filter.MinRSI) return false;
                if (filter.MaxRSI.HasValue && rsi > filter.MaxRSI) return false;
                return true;
            });
        }

        // ── Technical: Beta ───────────────────────────────────────────────────
        if (filter.MinBeta.HasValue)
            result = result.Where(s => s.Beta >= filter.MinBeta);
        if (filter.MaxBeta.HasValue)
            result = result.Where(s => s.Beta <= filter.MaxBeta);

        // ── Sort ──────────────────────────────────────────────────────────────
        result = Sort(result, filter.SortBy, filter.SortOrder);

        // ── Paginate ──────────────────────────────────────────────────────────
        int skip = (filter.Page - 1) * filter.PageSize;
        return result.Skip(skip).Take(filter.PageSize);
    }

    private static IEnumerable<Stock> Sort(IEnumerable<Stock> stocks, string? sortBy, SortOrder order)
    {
        bool asc = order == SortOrder.Ascending;
        return sortBy?.ToLowerInvariant() switch
        {
            "price"       => asc ? stocks.OrderBy(s => s.CurrentPrice)               : stocks.OrderByDescending(s => s.CurrentPrice),
            "marketcap"   => asc ? stocks.OrderBy(s => s.MarketCap)                  : stocks.OrderByDescending(s => s.MarketCap),
            "volume"      => asc ? stocks.OrderBy(s => s.Volume)                     : stocks.OrderByDescending(s => s.Volume),
            "pe"          => asc ? stocks.OrderBy(s => s.Fundamentals?.PE_Ratio)     : stocks.OrderByDescending(s => s.Fundamentals?.PE_Ratio),
            "companyname" => asc ? stocks.OrderBy(s => s.CompanyName)                : stocks.OrderByDescending(s => s.CompanyName),
            _             => asc ? stocks.OrderBy(s => s.Symbol)                     : stocks.OrderByDescending(s => s.Symbol),
        };
    }
}
