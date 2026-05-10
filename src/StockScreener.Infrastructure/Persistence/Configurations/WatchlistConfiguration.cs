using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockScreener.Domain.Entities;

namespace StockScreener.Infrastructure.Persistence.Configurations;

public class WatchlistConfiguration : IEntityTypeConfiguration<Watchlist>
{
    public void Configure(EntityTypeBuilder<Watchlist> builder)
    {
        builder.ToTable("Watchlists");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Description).HasMaxLength(500);

        builder.Property(w => w.UserId)
            .IsRequired()
            .HasMaxLength(128);

        // Index for user-scoped queries
        builder.HasIndex(w => w.UserId)
            .HasDatabaseName("IX_Watchlists_UserId");

        builder.HasMany(w => w.Items)
            .WithOne(i => i.Watchlist)
            .HasForeignKey(i => i.WatchlistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
