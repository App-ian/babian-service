using System;
using System.Collections.Generic;

namespace Babian.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "Barman";
    public string? RestaurantId { get; set; }
    public string? ApiKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Drink> Drinks { get; set; } = new List<Drink>();
    public ICollection<MarketSession> MarketSessions { get; set; } = new List<MarketSession>();
}
