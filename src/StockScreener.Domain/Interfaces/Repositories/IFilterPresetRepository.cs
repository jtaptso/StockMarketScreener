using StockScreener.Domain.Entities;

namespace StockScreener.Domain.Interfaces.Repositories;

public interface IFilterPresetRepository
{
    Task<FilterPreset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FilterPreset>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(FilterPreset preset, CancellationToken cancellationToken = default);
    Task UpdateAsync(FilterPreset preset, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
