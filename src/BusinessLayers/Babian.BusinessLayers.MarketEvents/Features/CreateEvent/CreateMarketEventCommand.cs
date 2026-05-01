using Babian.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;

namespace Babian.BusinessLayers.MarketEvents.Features.CreateEvent;

public record CreateMarketEventCommand(
    Guid BarmanId,
    string Name,
    string? Description,
    MarketEventType Type,
    decimal Value,
    DateTime StartAt,
    DateTime EndAt,
    List<Guid> DrinkIds,
    bool IsTemplate,
    MarketEventStatus Status,
    RecurrenceType Recurrence = RecurrenceType.None
) : IRequest<MarketEvent>;
