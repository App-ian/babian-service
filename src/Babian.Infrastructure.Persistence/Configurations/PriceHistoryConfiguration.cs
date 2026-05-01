using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Babian.Infrastructure.Persistence.Configurations;

public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.ToTable("price_histories");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(x => x.Price)
            .HasColumnName("price")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.Variation)
            .HasColumnName("variation")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.CycleNumber)
            .HasColumnName("cycle_number")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(x => x.DrinkId)
            .HasColumnName("drink_id")
            .IsRequired();

        builder.Property(x => x.MarketSessionId)
            .HasColumnName("market_session_id");

        builder.HasOne(x => x.Drink)
            .WithMany()
            .HasForeignKey(x => x.DrinkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MarketSession)
            .WithMany()
            .HasForeignKey(x => x.MarketSessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.DrinkId);
    }
}
