using System;

namespace Babian.BusinessLayers.MarketSessions.Features.GetActiveSession;

public record MarketSessionDto(
    Guid Id,
    string Name,
    DateTime StartTime,
    DateTime? EndTime,
    bool IsActive,
    DateTime CreatedAt,
    int CurrentCycleNumber,
    DateTime LastPriceUpdateAt,
    Guid OwnerId
);
