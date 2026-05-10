namespace StockScreener.Application.DTOs;

public record FilterPresetDto(
    Guid Id,
    string Name,
    string? Description,
    ScreenerFilterDto Filter,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
