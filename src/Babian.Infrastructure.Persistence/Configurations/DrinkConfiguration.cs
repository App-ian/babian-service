using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Babian.Infrastructure.Persistence.Configurations;

public class DrinkConfiguration : IEntityTypeConfiguration<Drink>
{
    public void Configure(EntityTypeBuilder<Drink> builder)
    {
        builder.ToTable("drinks");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(x => x.BasePrice)
            .HasColumnName("base_price")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.CurrentPrice)
            .HasColumnName("current_price")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.MinPrice)
            .HasColumnName("min_price")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.MaxPrice)
            .HasColumnName("max_price")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.MarketPrice)
            .HasColumnName("market_price")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasColumnName("image_url")
            .HasColumnType("longtext");

        builder.Property(x => x.Volume)
            .HasColumnName("volume");

        builder.Property(x => x.RowVersion)
            .HasColumnName("row_version")
            .IsConcurrencyToken();

        builder.Property(x => x.Category)
            .HasColumnName("category");

        builder.Property(x => x.Plu)
            .HasColumnName("plu");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(x => x.GlobalDrinkId)
            .HasColumnName("global_drink_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        // Relationships
        builder.HasOne(x => x.GlobalDrink)
            .WithMany()
            .HasForeignKey(x => x.GlobalDrinkId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(x => x.OwnerId);
    }
}
