namespace StockScreener.Domain.Entities;

public class Watchlist
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public ICollection<WatchlistItem> Items { get; init; } = new List<WatchlistItem>();
}
