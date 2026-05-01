using System;

namespace Babian.Domain.Entities;

public class PriceHistory
{
    public Guid Id { get; set; }
    public Guid DrinkId { get; set; }
    public Drink? Drink { get; set; }
    
    public decimal Price { get; set; }
    public decimal Variation { get; set; }
    public int CycleNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? MarketSessionId { get; set; }
    public MarketSession? MarketSession { get; set; }
}
