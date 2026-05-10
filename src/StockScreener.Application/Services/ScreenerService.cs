using System.Text.Json;
using StockScreener.Application.DTOs;
using StockScreener.Application.Interfaces;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Domain.Interfaces.Services;
using StockScreener.Domain.ValueObjects;

namespace StockScreener.Application.Services;

public class ScreenerService(
    IStockRepository stockRepository,
    IScreenerEngine screenerEngine,
    IFilterPresetRepository presetRepository) : IScreenerService
{
    public async Task<ScreenerResultDto> FilterAsync(
        ScreenerFilterDto dto,
        CancellationToken cancellationToken = default)
    {
        var stocks = await stockRepository.GetAllAsync(cancellationToken);

        // Build a filter with no pagination to get the full sorted+filtered list,
        // then paginate here so we can return the accurate total count.
        var countFilter = MapToFilter(dto) with { Page = 1, PageSize = int.MaxValue };
        var allFiltered = screenerEngine.Filter(stocks, countFilter).ToList();

        int totalCount = allFiltered.Count;
        int totalPages = (int)Math.Ceiling(totalCount / (double)dto.PageSize);

        var pageItems = allFiltered
            .Skip((dto.Page - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .Select(MapStockToDto)
            .ToList();

        return new ScreenerResultDto(pageItems, new PageInfo(dto.Page, dto.PageSize, totalCount, totalPages));
    }

    public async Task<IEnumerable<FilterPresetDto>> GetPresetsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var presets = await presetRepository.GetByUserIdAsync(userId, cancellationToken);
        return presets.Select(MapPresetToDto);
    }

    public async Task<FilterPresetDto> SavePresetAsync(
        string userId,
        string name,
        string? description,
        ScreenerFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var preset = new FilterPreset
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            FilterJson = JsonSerializer.Serialize(filter),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await presetRepository.AddAsync(preset, cancellationToken);
        return MapPresetToDto(preset);
    }

    public Task DeletePresetAsync(Guid presetId, CancellationToken cancellationToken = default)
        => presetRepository.DeleteAsync(presetId, cancellationToken);

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static ScreenerFilter MapToFilter(ScreenerFilterDto dto) => new()
    {
        MinPrice = dto.MinPrice,
        MaxPrice = dto.MaxPrice,
        MinMarketCap = dto.MinMarketCap,
        MaxMarketCap = dto.MaxMarketCap,
        MarketCapCategories = dto.MarketCapCategories,
        Exchanges = dto.Exchanges,
        Sectors = dto.Sectors,
        MinVolume = dto.MinVolume,
        MaxVolume = dto.MaxVolume,
        MinPE = dto.MinPE,
        MaxPE = dto.MaxPE,
        MinPB = dto.MinPB,
        MaxPB = dto.MaxPB,
        MinDividendYield = dto.MinDividendYield,
        MaxDividendYield = dto.MaxDividendYield,
        MinRSI = dto.MinRSI,
        MaxRSI = dto.MaxRSI,
        MinBeta = dto.MinBeta,
        MaxBeta = dto.MaxBeta,
        SortBy = dto.SortBy,
        SortOrder = dto.SortOrder,
        Page = dto.Page,
        PageSize = dto.PageSize
    };

    private static StockDto MapStockToDto(Stock s) => new(
        s.Id, s.Symbol, s.CompanyName, s.Exchange,
        s.Sector, s.Industry, s.MarketCap, s.MarketCapCategory,
        s.CurrentPrice, s.DayHigh, s.DayLow, s.Week52High, s.Week52Low,
        s.Volume, s.AvgVolume, s.Beta, s.LastUpdated);

    private static FilterPresetDto MapPresetToDto(FilterPreset p)
    {
        var filter = string.IsNullOrWhiteSpace(p.FilterJson)
            ? new ScreenerFilterDto()
            : JsonSerializer.Deserialize<ScreenerFilterDto>(p.FilterJson) ?? new ScreenerFilterDto();

        return new FilterPresetDto(p.Id, p.Name, p.Description, filter, p.CreatedAt, p.UpdatedAt);
    }
}
