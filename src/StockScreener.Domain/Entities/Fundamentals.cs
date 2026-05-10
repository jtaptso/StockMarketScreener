namespace StockScreener.Domain.Entities;

public class Fundamentals
{
    public int Id { get; init; }
    public int StockId { get; init; }

    // Valuation
    public decimal? PE_Ratio { get; init; }
    public decimal? PB_Ratio { get; init; }
    public decimal? PS_Ratio { get; init; }
    public decimal? EPS { get; init; }

    // Dividends
    public decimal? DividendYield { get; init; }
    public DateOnly? ExDividendDate { get; init; }

    // Financial Health
    public decimal? DebtToEquity { get; init; }
    public decimal? CurrentRatio { get; init; }
    public decimal? QuickRatio { get; init; }

    // Profitability
    public decimal? ROE { get; init; }
    public decimal? ProfitMargin { get; init; }
    public decimal? OperatingMargin { get; init; }
    public decimal? GrossMargin { get; init; }

    // Growth
    public decimal? RevenueGrowth { get; init; }
    public decimal? EPSGrowth { get; init; }

    // Financials
    public decimal? Revenue { get; init; }
    public decimal? NetIncome { get; init; }
    public decimal? TotalDebt { get; init; }
    public decimal? TotalEquity { get; init; }

    public DateOnly? FiscalYearEnd { get; init; }
    public DateTime LastUpdated { get; init; }

    // Navigation property
    public Stock Stock { get; init; } = null!;
}
