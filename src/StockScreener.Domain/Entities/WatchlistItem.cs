namespace StockScreener.Domain.Entities;

public class WatchlistItem
{
    public long Id { get; init; }
    public Guid WatchlistId { get; init; }
    public int StockId { get; init; }
    public decimal? SharesOwned { get; set; }
    public decimal? CostBasis { get; set; }
    public DateTime AddedAt { get; init; }

    // Navigation properties
    public Watchlist Watchlist { get; init; } = null!;
    public Stock Stock { get; init; } = null!;
}
