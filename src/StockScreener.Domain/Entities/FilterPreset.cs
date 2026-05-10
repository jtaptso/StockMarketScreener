namespace StockScreener.Domain.Entities;

public class FilterPreset
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FilterJson { get; set; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
}
