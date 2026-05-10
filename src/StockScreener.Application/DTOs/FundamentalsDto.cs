namespace StockScreener.Application.DTOs;

public record FundamentalsDto(
    int Id,
    int StockId,
    // Valuation
    decimal? PE_Ratio,
    decimal? PB_Ratio,
    decimal? PS_Ratio,
    decimal? EPS,
    // Dividends
    decimal? DividendYield,
    DateOnly? ExDividendDate,
    // Financial Health
    decimal? DebtToEquity,
    decimal? CurrentRatio,
    decimal? QuickRatio,
    // Profitability
    decimal? ROE,
    decimal? ProfitMargin,
    decimal? OperatingMargin,
    decimal? GrossMargin,
    // Growth
    decimal? RevenueGrowth,
    decimal? EPSGrowth,
    // Financials
    decimal? Revenue,
    decimal? NetIncome,
    decimal? TotalDebt,
    decimal? TotalEquity,
    DateOnly? FiscalYearEnd,
    DateTime LastUpdated
);
