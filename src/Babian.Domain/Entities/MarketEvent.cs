using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Babian.Domain.Entities;

public class MarketEvent
{
    public Guid Id { get; set; }
    public Guid BarmanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MarketEventType Type { get; set; }
    public decimal Value { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public List<Guid> DrinkIds { get; set; } = new();
    public bool IsTemplate { get; set; }
    public MarketEventStatus Status { get; set; }
    public Guid? MarketSessionId { get; set; }
    
    // Recurrence
    public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;
    public Guid? ParentEventId { get; set; }
}

public enum RecurrenceType
{
    None,
    Daily,
    Weekly,
    Monthly,
    Yearly
}

public enum MarketEventType
{
    [JsonPropertyName("discount")] Discount,
    [JsonPropertyName("fixed_price")] FixedPrice
}

public enum MarketEventStatus
{
    [JsonPropertyName("active")] Active,
    [JsonPropertyName("scheduled")] Scheduled,
    [JsonPropertyName("live")] Live,
    [JsonPropertyName("finished")] Finished,
    [JsonPropertyName("template")] Template
}
