namespace StockScreener.Application.DTOs;

public record WatchlistItemDto(
    long Id,
    int StockId,
    string Symbol,
    string CompanyName,
    decimal? CurrentPrice,
    decimal? SharesOwned,
    decimal? CostBasis,
    DateTime AddedAt
);

public record WatchlistDto(
    Guid Id,
    string Name,
    string? Description,
    string UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<WatchlistItemDto> Items
);
