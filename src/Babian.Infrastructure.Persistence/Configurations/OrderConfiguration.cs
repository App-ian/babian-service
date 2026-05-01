using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Babian.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.PriceAtOrder)
            .HasColumnName("price_at_order")
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

        builder.Property(x => x.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.HasOne(x => x.Drink)
            .WithMany()
            .HasForeignKey(x => x.DrinkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MarketSession)
            .WithMany()
            .HasForeignKey(x => x.MarketSessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.OwnerId);
    }
}
