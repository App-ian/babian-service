using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Babian.Infrastructure.Persistence.Configurations;

public class GlobalDrinkConfiguration : IEntityTypeConfiguration<GlobalDrink>
{
    public void Configure(EntityTypeBuilder<GlobalDrink> builder)
    {
        builder.ToTable("global_drinks");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(x => x.DefaultBasePrice)
            .HasColumnName("default_base_price")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasColumnName("image_url")
            .HasColumnType("longtext");

        builder.Property(x => x.Volume)
            .HasColumnName("volume");

        builder.Property(x => x.Category)
            .HasColumnName("category");

        builder.Property(x => x.Plu)
            .HasColumnName("plu");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");
    }
}
