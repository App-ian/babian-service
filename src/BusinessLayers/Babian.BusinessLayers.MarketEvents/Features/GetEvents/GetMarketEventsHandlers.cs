using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using Babian.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Babian.BusinessLayers.MarketEvents.Features.GetEvents;

public class GetMarketEventsHandlers : 
    IRequestHandler<GetMarketEventsQuery, List<MarketEvent>>,
    IRequestHandler<GetActiveMarketEventsQuery, List<MarketEvent>>,
    IRequestHandler<GetTemplateMarketEventsQuery, List<MarketEvent>>
{
    private readonly AppDbContext _context;

    public GetMarketEventsHandlers(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<MarketEvent>> Handle(GetMarketEventsQuery request, CancellationToken cancellationToken)
    {
        return await _context.MarketEvents
            .Where(e => e.BarmanId == request.BarmanId)
            .OrderByDescending(e => e.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MarketEvent>> Handle(GetActiveMarketEventsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var activeSession = await _context.MarketSessions
            .Where(s => s.OwnerId == request.BarmanId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSession == null) return new List<MarketEvent>();

        var lastUpdate = activeSession.LastPriceUpdateAt;

        return await _context.MarketEvents
            .Where(e => e.BarmanId == request.BarmanId && 
                        e.MarketSessionId == activeSession.Id &&
                        (
                            // Cas 1 : L'événement est en cours
                            ((e.Status == MarketEventStatus.Active || e.Status == MarketEventStatus.Scheduled) && 
                             e.StartAt <= now && e.EndAt >= now)
                            ||
                            // Cas 2 : L'événement vient d'être fini manuellement, 
                            // mais il impactait encore le dernier calcul de prix affiché
                            (e.Status == MarketEventStatus.Finished && e.EndAt > lastUpdate)
                        ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MarketEvent>> Handle(GetTemplateMarketEventsQuery request, CancellationToken cancellationToken)
    {
        return await _context.MarketEvents
            .Where(e => e.BarmanId == request.BarmanId && e.IsTemplate)
            .ToListAsync(cancellationToken);
    }
}
