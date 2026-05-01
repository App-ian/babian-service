using System;
using System.Collections.Generic;

namespace Babian.Domain.Entities;

public class MarketConfig
{
    public Guid Id { get; set; }
    public Guid BarmanId { get; set; }
    public User? Barman { get; set; }
    public int CycleDurationSeconds { get; set; } = 60;
    public int? TotalCycles { get; set; }
    
    // Legacy metrics from the old schema
    public decimal DecreaseOthers { get; set; } = 0.05m;
    public decimal IncreasePerOrder { get; set; } = 0.10m;
    
    // Stored as JSONB in PostgreSQL/MariaDB
    public List<RankingGroup> RankingGroups { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
