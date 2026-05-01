using System;
using MediatR;
using Babian.Domain.Entities;

namespace Babian.BusinessLayers.MarketEvents.Features.ActivateEvent;

/// <summary>
/// Commande permettant d'activer un événement à partir de son template.
/// </summary>
public record ActivateMarketEventCommand(
    Guid EventId, 
    int DurationMinutes, 
    Guid BarmanId
) : IRequest<MarketEvent>;
