using StockScreener.Application.DTOs;
using StockScreener.Application.Interfaces;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Interfaces.Repositories;

namespace StockScreener.Application.Services;

public class StockService(
    IStockRepository stockRepository,
    IPriceHistoryRepository priceHistoryRepository,
    IFundamentalsRepository fundamentalsRepository) : IStockService
{
    public async Task<StockDto?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        var stock = await stockRepository.GetBySymbolAsync(symbol, cancellationToken);
        return stock is null ? null : MapStockToDto(stock);
    }

    public async Task<IEnumerable<PriceHistoryDto>> GetPriceHistoryAsync(
        string symbol,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var stock = await stockRepository.GetBySymbolAsync(symbol, cancellationToken);
        if (stock is null)
            return Enumerable.Empty<PriceHistoryDto>();

        var history = await priceHistoryRepository.GetByStockIdAndDateRangeAsync(
            stock.Id, from, to, cancellationToken);

        return history.Select(MapPriceHistoryToDto);
    }

    public async Task<FundamentalsDto?> GetFundamentalsAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        var stock = await stockRepository.GetBySymbolAsync(symbol, cancellationToken);
        if (stock is null)
            return null;

        var fundamentals = await fundamentalsRepository.GetByStockIdAsync(stock.Id, cancellationToken);
        return fundamentals is null ? null : MapFundamentalsToDto(fundamentals);
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static StockDto MapStockToDto(Stock s) => new(
        s.Id, s.Symbol, s.CompanyName, s.Exchange,
        s.Sector, s.Industry, s.MarketCap, s.MarketCapCategory,
        s.CurrentPrice, s.DayHigh, s.DayLow, s.Week52High, s.Week52Low,
        s.Volume, s.AvgVolume, s.Beta, s.LastUpdated);

    private static PriceHistoryDto MapPriceHistoryToDto(PriceHistory p) => new(
        p.Id, p.StockId, p.TradeDate,
        p.OpenPrice, p.HighPrice, p.LowPrice, p.ClosePrice, p.Volume,
        p.PriceChange, p.PriceChangePercent, p.Range);

    private static FundamentalsDto MapFundamentalsToDto(Fundamentals f) => new(
        f.Id, f.StockId,
        f.PE_Ratio, f.PB_Ratio, f.PS_Ratio, f.EPS,
        f.DividendYield, f.ExDividendDate,
        f.DebtToEquity, f.CurrentRatio, f.QuickRatio,
        f.ROE, f.ProfitMargin, f.OperatingMargin, f.GrossMargin,
        f.RevenueGrowth, f.EPSGrowth,
        f.Revenue, f.NetIncome, f.TotalDebt, f.TotalEquity,
        f.FiscalYearEnd, f.LastUpdated);
}
