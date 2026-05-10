namespace StockScreener.Application.DTOs;

public record PageInfo(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public record ScreenerResultDto(
    IReadOnlyList<StockDto> Items,
    PageInfo PageInfo
);
