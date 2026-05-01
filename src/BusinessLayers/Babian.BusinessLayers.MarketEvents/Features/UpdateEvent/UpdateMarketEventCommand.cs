using System;
using System.Collections.Generic;
using Babian.Domain.Entities;
using MediatR;

namespace Babian.BusinessLayers.MarketEvents.Features.UpdateEvent;

public class UpdateMarketEventCommand : IRequest<bool>
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
    public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;
}
