using System;

namespace Babian.Domain.Entities;

public class Drink
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal MarketPrice { get; set; }
    public string? ImageUrl { get; set; }
    public string? Volume { get; set; }
    public string? Category { get; set; }
    public string? Plu { get; set; }
    public bool IsActive { get; set; } = true;
    
    public Guid OwnerId { get; set; } // Barman ID (FK to User)
    public User? Owner { get; set; }

    public Guid? MarketSessionId { get; set; }
    public MarketSession? MarketSession { get; set; }

    public Guid GlobalDrinkId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    // Navigation properties
    public GlobalDrink? GlobalDrink { get; set; }
}
