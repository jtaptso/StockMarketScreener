using FluentAssertions;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Domain.Services;
using StockScreener.Domain.ValueObjects;

namespace StockScreener.Tests.Domain;

public class ScreenerEngineTests
{
    private readonly ScreenerEngine _sut = new();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Stock MakeStock(
        int id,
        string symbol,
        decimal? price = 100m,
        decimal? marketCap = 5_000_000_000m,
        Exchange exchange = Exchange.NYSE,
        string? sector = "Technology",
        long? volume = 1_000_000L,
        decimal? pe = 20m,
        decimal? pb = 3m,
        decimal? dividendYield = 1m,
        decimal? rsi = 50m,
        decimal? beta = 1m) =>
        new()
        {
            Id            = id,
            Symbol        = symbol,
            CompanyName   = $"Company {symbol}",
            Exchange      = exchange,
            Sector        = sector,
            MarketCap     = marketCap,
            CurrentPrice  = price,
            Volume        = volume,
            Beta          = beta,
            Fundamentals  = new Fundamentals
            {
                Id            = id,
                StockId       = id,
                PE_Ratio      = pe,
                PB_Ratio      = pb,
                DividendYield = dividendYield,
            },
            TechnicalIndicators = rsi.HasValue
                ? new List<TechnicalIndicators>
                  {
                      new() { StockId = id, TradeDate = DateOnly.FromDateTime(DateTime.Today), RSI_14 = rsi }
                  }
                : new List<TechnicalIndicators>(),
        };

    private static ScreenerFilter EmptyFilter(int pageSize = 100) =>
        new() { Page = 1, PageSize = pageSize };

    // ── No-filter pass-through ────────────────────────────────────────────────

    [Fact]
    public void Filter_ReturnsAll_WhenNoFiltersApplied()
    {
        var stocks = new[] { MakeStock(1, "AAPL"), MakeStock(2, "MSFT") };
        _sut.Filter(stocks, EmptyFilter()).Should().HaveCount(2);
    }

    // ── Price ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Filter_ByMinPrice_ExcludesBelowThreshold()
    {
        var stocks = new[]
        {
            MakeStock(1, "LOW",  price: 50m),
            MakeStock(2, "HIGH", price: 200m),
        };
        var result = _sut.Filter(stocks, new ScreenerFilter { MinPrice = 100m, PageSize = 100 });
        result.Should().ContainSingle(s => s.Symbol == "HIGH");
    }

    [Fact]
    public void Filter_ByMaxPrice_ExcludesAboveThreshold()
    {
        var stocks = new[]
        {
            MakeStock(1, "LOW",  price: 50m),
            MakeStock(2, "HIGH", price: 200m),
        };
        var result = _sut.Filter(stocks, new ScreenerFilter { MaxPrice = 100m, PageSize = 100 });
        result.Should().ContainSingle(s => s.Symbol == "LOW");
    }

    [Fact]
    public void Filter_ByPriceRange_ReturnsOnlyStocksWithinRange()
    {
        var stocks = new[]
        {
            MakeStock(1, "A", price: 50m),
            MakeStock(2, "B", price: 150m),
            MakeStock(3, "C", price: 300m),
        };
        var result = _sut.Filter(stocks, new ScreenerFilter { MinPrice = 100m, MaxPrice = 200m, PageSize = 100 });
        result.Should().ContainSingle(s => s.Symbol == "B");
    }

    // ── Exchange ──────────────────────────────────────────────────────────────

    [Fact]
    public void Filter_ByExchange_ReturnsOnlyMatchingExchange()
    {
        var stocks = new[]
        {
            MakeStock(1, "NYSE_STOCK",   exchange: Exchange.NYSE),
            MakeStock(2, "NASDAQ_STOCK", exchange: Exchange.NASDAQ),
        };
        var result = _sut.Filter(stocks,
            new ScreenerFilter { Exchanges = new[] { Exchange.NASDAQ }, PageSize = 100 });
        result.Should().ContainSingle(s => s.Symbol == "NASDAQ_STOCK");
    }

    // ── Sector ────────────────────────────────────────────────────────────────

    [Fact]
    public void Filter_BySector_ReturnsOnlyMatchingSector()
    {
        var stocks = new[]
        {
            MakeStock(1, "TECH",   sector: "Technology"),
            MakeStock(2, "ENERGY", sector: "Energy"),
        };
        var result = _sut.Filter(stocks,
            new ScreenerFilter { Sectors = new[] { "Energy" }, PageSize = 100 });
        result.Should().ContainSingle(s => s.Symbol == "ENERGY");
    }

    // ── Market Cap Category ───────────────────────────────────────────────────

    [Fact]
    public void Filter_ByMarketCapCategory_ReturnsOnlyMatchingCategory()
    {
        var stocks = new[]
        {
            // Small cap: < 2B
            MakeStock(1, "SMALL", marketCap: 500_000_000m),
            // Large cap: < 200B
            MakeStock(2, "LARGE", marketCap: 50_000_000_000m),
        };
        var result = _sut.Filter(stocks,
            new ScreenerFilter
            {
                MarketCapCategories = new[] { MarketCapCategory.Small },
                PageSize = 100,
            });
        result.Should().ContainSingle(s => s.Symbol == "SMALL");
    }

    // ── Fundamentals ──────────────────────────────────────────────────────────

    [Fact]
    public void Filter_ByPERange_ExcludesStocksOutsideRange()
    {
        var stocks = new[]
        {
            MakeStock(1, "LOW_PE",  pe: 10m),
            MakeStock(2, "MID_PE",  pe: 20m),
            MakeStock(3, "HIGH_PE", pe: 50m),
        };
        var result = _sut.Filter(stocks,
            new ScreenerFilter { MinPE = 15m, MaxPE = 30m, PageSize = 100 });
        result.Should().ContainSingle(s => s.Symbol == "MID_PE");
    }

    [Fact]
    public void Filter_ByPE_ExcludesStocksWithNoFundamentals()
    {
        var noFundamentals = new Stock
        {
            Id = 99, Symbol = "NO_FUND", CompanyName = "No Fund",
            Exchange = Exchange.NYSE, CurrentPrice = 100m,
        };
        var stocks = new[] { noFundamentals, MakeStock(1, "WITH_FUND", pe: 20m) };
        var result = _sut.Filter(stocks,
            new ScreenerFilter { MinPE = 1m, PageSize = 100 });
        result.Should().ContainSingle(s => s.Symbol == "WITH_FUND");
    }

    // ── Technical: RSI ────────────────────────────────────────────────────────

    [Fact]
    public void Filter_ByRSIRange_ReturnsOnlyStocksWithinRange()
    {
        var stocks = new[]
        {
            MakeStock(1, "OVERSOLD",   rsi: 25m),
            MakeStock(2, "NEUTRAL",    rsi: 55m),
            MakeStock(3, "OVERBOUGHT", rsi: 80m),
        };
        var result = _sut.Filter(stocks,
            new ScreenerFilter { MinRSI = 30m, MaxRSI = 70m, PageSize = 100 });
        result.Should().ContainSingle(s => s.Symbol == "NEUTRAL");
    }

    [Fact]
    public void Filter_ByRSI_ExcludesStocksWithNoTechnicalIndicators()
    {
        var stocks = new[]
        {
            MakeStock(1, "NO_RSI", rsi: null),
            MakeStock(2, "HAS_RSI", rsi: 50m),
        };
        var result = _sut.Filter(stocks,
            new ScreenerFilter { MinRSI = 40m, PageSize = 100 });
        result.Should().ContainSingle(s => s.Symbol == "HAS_RSI");
    }

    // ── AND Logic ─────────────────────────────────────────────────────────────

    [Fact]
    public void Filter_MultipleCriteria_AppliesAndLogic()
    {
        var stocks = new[]
        {
            MakeStock(1, "BOTH",      price: 150m, exchange: Exchange.NASDAQ),
            MakeStock(2, "PRICE_OK",  price: 150m, exchange: Exchange.NYSE),
            MakeStock(3, "EXCH_OK",   price: 50m,  exchange: Exchange.NASDAQ),
        };
        var result = _sut.Filter(stocks, new ScreenerFilter
        {
            MinPrice  = 100m,
            Exchanges = new[] { Exchange.NASDAQ },
            PageSize  = 100,
        });
        result.Should().ContainSingle(s => s.Symbol == "BOTH");
    }

    // ── Sorting ───────────────────────────────────────────────────────────────

    [Fact]
    public void Filter_SortByPriceAscending_ReturnsOrderedByPriceAsc()
    {
        var stocks = new[]
        {
            MakeStock(1, "C", price: 300m),
            MakeStock(2, "A", price: 100m),
            MakeStock(3, "B", price: 200m),
        };
        var result = _sut.Filter(stocks,
            new ScreenerFilter { SortBy = "price", SortOrder = SortOrder.Ascending, PageSize = 100 })
            .ToList();
        result.Select(s => s.CurrentPrice).Should().BeInAscendingOrder();
    }

    [Fact]
    public void Filter_SortByPriceDescending_ReturnsOrderedByPriceDesc()
    {
        var stocks = new[]
        {
            MakeStock(1, "C", price: 300m),
            MakeStock(2, "A", price: 100m),
            MakeStock(3, "B", price: 200m),
        };
        var result = _sut.Filter(stocks,
            new ScreenerFilter { SortBy = "price", SortOrder = SortOrder.Descending, PageSize = 100 })
            .ToList();
        result.Select(s => s.CurrentPrice).Should().BeInDescendingOrder();
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    [Fact]
    public void Filter_Pagination_ReturnsCorrectPage()
    {
        var stocks = Enumerable.Range(1, 10)
            .Select(i => MakeStock(i, $"S{i:D2}"))
            .ToArray();

        var page1 = _sut.Filter(stocks, new ScreenerFilter { Page = 1, PageSize = 3 }).ToList();
        var page2 = _sut.Filter(stocks, new ScreenerFilter { Page = 2, PageSize = 3 }).ToList();

        page1.Should().HaveCount(3);
        page2.Should().HaveCount(3);
        page1.Select(s => s.Symbol).Should().NotIntersectWith(page2.Select(s => s.Symbol));
    }

    [Fact]
    public void Filter_LastPage_ReturnsRemainingItems()
    {
        var stocks = Enumerable.Range(1, 7)
            .Select(i => MakeStock(i, $"S{i:D2}"))
            .ToArray();

        // Page 3 of 3 with pageSize=3 → only 1 item
        var result = _sut.Filter(stocks, new ScreenerFilter { Page = 3, PageSize = 3 }).ToList();
        result.Should().HaveCount(1);
    }
}
