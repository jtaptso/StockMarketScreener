using StockScreener.Application.DTOs;

namespace StockScreener.Application.Interfaces;

public interface IScreenerService
{
    Task<ScreenerResultDto> FilterAsync(
        ScreenerFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<FilterPresetDto>> GetPresetsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<FilterPresetDto> SavePresetAsync(
        string userId,
        string name,
        string? description,
        ScreenerFilterDto filter,
        CancellationToken cancellationToken = default);

    Task DeletePresetAsync(Guid presetId, CancellationToken cancellationToken = default);
}
