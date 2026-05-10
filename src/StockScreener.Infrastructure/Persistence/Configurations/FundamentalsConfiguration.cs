using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockScreener.Domain.Entities;

namespace StockScreener.Infrastructure.Persistence.Configurations;

public class FundamentalsConfiguration : IEntityTypeConfiguration<Fundamentals>
{
    public void Configure(EntityTypeBuilder<Fundamentals> builder)
    {
        builder.ToTable("Fundamentals");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.PE_Ratio).HasColumnType("decimal(18,4)");
        builder.Property(f => f.PB_Ratio).HasColumnType("decimal(18,4)");
        builder.Property(f => f.PS_Ratio).HasColumnType("decimal(18,4)");
        builder.Property(f => f.EPS).HasColumnType("decimal(18,4)");
        builder.Property(f => f.DividendYield).HasColumnType("decimal(10,4)");
        builder.Property(f => f.DebtToEquity).HasColumnType("decimal(18,4)");
        builder.Property(f => f.CurrentRatio).HasColumnType("decimal(10,4)");
        builder.Property(f => f.QuickRatio).HasColumnType("decimal(10,4)");
        builder.Property(f => f.ROE).HasColumnType("decimal(10,4)");
        builder.Property(f => f.ProfitMargin).HasColumnType("decimal(10,4)");
        builder.Property(f => f.OperatingMargin).HasColumnType("decimal(10,4)");
        builder.Property(f => f.GrossMargin).HasColumnType("decimal(10,4)");
        builder.Property(f => f.RevenueGrowth).HasColumnType("decimal(10,4)");
        builder.Property(f => f.EPSGrowth).HasColumnType("decimal(10,4)");
        builder.Property(f => f.Revenue).HasColumnType("decimal(20,2)");
        builder.Property(f => f.NetIncome).HasColumnType("decimal(20,2)");
        builder.Property(f => f.TotalDebt).HasColumnType("decimal(20,2)");
        builder.Property(f => f.TotalEquity).HasColumnType("decimal(20,2)");

        // Index for stock lookups
        builder.HasIndex(f => f.StockId)
            .HasDatabaseName("IX_Fundamentals_StockId");
    }
}
