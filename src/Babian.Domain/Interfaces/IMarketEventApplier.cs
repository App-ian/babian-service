using System;
using System.Collections.Generic;
using Babian.Domain.Entities;

namespace Babian.Domain.Interfaces;

public interface IMarketEventApplier
{
    decimal ApplyEvents(Drink drink, decimal pureMarketPrice, Guid currentSessionId, IEnumerable<MarketEvent> activeEvents);
}
