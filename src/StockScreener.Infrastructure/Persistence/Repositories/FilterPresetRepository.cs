using Microsoft.EntityFrameworkCore;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Infrastructure.Persistence;

namespace StockScreener.Infrastructure.Persistence.Repositories;

public class FilterPresetRepository(AppDbContext db) : IFilterPresetRepository
{
    public async Task<FilterPreset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await db.FilterPresets
            .AsNoTracking()
            .FirstOrDefaultAsync(fp => fp.Id == id, cancellationToken);

    public async Task<IEnumerable<FilterPreset>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await db.FilterPresets
            .AsNoTracking()
            .Where(fp => fp.UserId == userId)
            .OrderBy(fp => fp.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(FilterPreset preset, CancellationToken cancellationToken = default)
    {
        await db.FilterPresets.AddAsync(preset, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FilterPreset preset, CancellationToken cancellationToken = default)
    {
        db.FilterPresets.Update(preset);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var preset = await db.FilterPresets.FindAsync([id], cancellationToken);
        if (preset is not null)
        {
            db.FilterPresets.Remove(preset);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
