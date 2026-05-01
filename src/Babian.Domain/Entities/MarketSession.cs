using System;

namespace Babian.Domain.Entities;

public class MarketSession
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CurrentCycleNumber { get; set; } = 1;
    public DateTime LastPriceUpdateAt { get; set; } = DateTime.UtcNow;
    
    // Le barman qui possède cette session
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }
}
