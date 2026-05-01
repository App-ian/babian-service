using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Babian.Infrastructure.Persistence.Configurations;

public class MarketConfigConfiguration : IEntityTypeConfiguration<MarketConfig>
{
    public void Configure(EntityTypeBuilder<MarketConfig> builder)
    {
        builder.ToTable("market_configs");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(x => x.BarmanId)
            .HasColumnName("barman_id")
            .IsRequired();

        builder.Property(x => x.CycleDurationSeconds)
            .HasColumnName("cycle_duration_seconds")
            .HasDefaultValue(60);

        builder.Property(x => x.TotalCycles)
            .HasColumnName("total_cycles");

        builder.Property(x => x.DecreaseOthers)
            .HasColumnName("decrease_others")
            .HasColumnType("numeric")
            .HasDefaultValue(0.05m);

        builder.Property(x => x.IncreasePerOrder)
            .HasColumnName("increase_per_order")
            .HasColumnType("numeric")
            .HasDefaultValue(0.10m);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        // System.Text.Json serialization mapped to JSON column
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        builder.Property(x => x.RankingGroups)
            .HasColumnName("ranking_groups")
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<System.Collections.Generic.List<RankingGroup>>(v, jsonOptions) ?? new()
            );

        builder.HasIndex(x => x.BarmanId).IsUnique();
    }
}
