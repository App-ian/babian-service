using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Babian.Infrastructure.Persistence.Configurations;

public class MarketEventConfiguration : IEntityTypeConfiguration<MarketEvent>
{
    public void Configure(EntityTypeBuilder<MarketEvent> builder)
    {
        builder.ToTable("market_events");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        
        builder.Property(x => x.DrinkIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>()
            );

        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.Type).HasConversion<string>();

        builder.Property(x => x.MarketSessionId)
            .HasColumnName("market_session_id")
            .IsRequired(false);

        // Performance Indexes
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.MarketSessionId);
        builder.HasIndex(x => x.StartAt);
        builder.HasIndex(x => x.BarmanId);
    }
}
