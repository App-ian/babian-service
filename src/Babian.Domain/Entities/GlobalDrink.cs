using System;

namespace Babian.Domain.Entities;

public class GlobalDrink
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DefaultBasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public string? Volume { get; set; }
    public string? Category { get; set; }
    public string? Plu { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
