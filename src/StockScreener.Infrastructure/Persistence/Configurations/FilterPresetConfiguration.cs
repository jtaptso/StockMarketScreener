using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockScreener.Domain.Entities;

namespace StockScreener.Infrastructure.Persistence.Configurations;

public class FilterPresetConfiguration : IEntityTypeConfiguration<FilterPreset>
{
    public void Configure(EntityTypeBuilder<FilterPreset> builder)
    {
        builder.ToTable("FilterPresets");

        builder.HasKey(fp => fp.Id);

        builder.Property(fp => fp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(fp => fp.Description).HasMaxLength(500);

        builder.Property(fp => fp.FilterJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(fp => fp.UserId)
            .IsRequired()
            .HasMaxLength(128);

        // Index for user-scoped queries
        builder.HasIndex(fp => fp.UserId)
            .HasDatabaseName("IX_FilterPresets_UserId");
    }
}
