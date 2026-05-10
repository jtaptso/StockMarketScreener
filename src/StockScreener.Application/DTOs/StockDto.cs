using StockScreener.Domain.Enums;

namespace StockScreener.Application.DTOs;

public record StockDto(
    int Id,
    string Symbol,
    string CompanyName,
    Exchange Exchange,
    string? Sector,
    string? Industry,
    decimal? MarketCap,
    MarketCapCategory MarketCapCategory,
    decimal? CurrentPrice,
    decimal? DayHigh,
    decimal? DayLow,
    decimal? Week52High,
    decimal? Week52Low,
    long? Volume,
    long? AvgVolume,
    decimal? Beta,
    DateTime LastUpdated
);
