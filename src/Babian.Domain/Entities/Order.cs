using System;

namespace Babian.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid DrinkId { get; set; }
    public Drink? Drink { get; set; }
    
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
    public int CycleNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? MarketSessionId { get; set; }
    public MarketSession? MarketSession { get; set; }

    // ID du barman
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }
}
