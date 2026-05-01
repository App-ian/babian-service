using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Babian.Infrastructure.Persistence.Configurations;

public class MarketSessionConfiguration : IEntityTypeConfiguration<MarketSession>
{
    public void Configure(EntityTypeBuilder<MarketSession> builder)
    {
        builder.ToTable("market_sessions");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnName("end_time");

        builder.Property(x => x.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.CurrentCycleNumber)
            .HasColumnName("current_cycle_number")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.LastPriceUpdateAt)
            .HasColumnName("last_price_update_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasIndex(x => x.OwnerId);
    }
}
