namespace StockScreener.Application.DTOs;

public record PriceHistoryDto(
    long Id,
    int StockId,
    DateOnly TradeDate,
    decimal OpenPrice,
    decimal HighPrice,
    decimal LowPrice,
    decimal ClosePrice,
    long Volume,
    decimal PriceChange,
    decimal PriceChangePercent,
    decimal Range
);
