using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.BusinessLayers.MarketEngine.Features.UpdatePrices;
using Babian.Domain.Entities;
using Babian.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Babian.Service.Workers;

public class MarketWorker : BackgroundService
{
    private readonly ILogger<MarketWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MarketWorker(ILogger<MarketWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MarketWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                // 1. Clôture des événements expirés
                var expiredEvents = await dbContext.MarketEvents
                    .Where(e => e.Status == MarketEventStatus.Live && e.EndAt <= DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                if (expiredEvents.Any())
                {
                    foreach (var ev in expiredEvents)
                    {
                        ev.Status = MarketEventStatus.Finished;
                        _logger.LogInformation("MarketWorker: Event {eventId} ({eventName}) finished.", ev.Id, ev.Name);
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                // 2. Auto-Cycle des Sessions
                var activeSessions = await dbContext.MarketSessions
                    .Where(s => s.IsActive)
                    .ToListAsync(stoppingToken);

                foreach (var session in activeSessions)
                {
                    var config = await dbContext.MarketConfigs
                        .FirstOrDefaultAsync(c => c.BarmanId == session.OwnerId, stoppingToken);

                    if (config == null) continue;

                    // Vérifier la limite de cycles si configurée
                    if (config.TotalCycles.HasValue && session.CurrentCycleNumber > config.TotalCycles.Value)
                    {
                        continue;
                    }

                    if (DateTime.UtcNow >= session.LastPriceUpdateAt.AddSeconds(config.CycleDurationSeconds))
                    {
                        _logger.LogInformation("MarketWorker: Triggering auto-cycle for session {sessionId} (Owner: {ownerId})", session.Id, session.OwnerId);
                        
                        await mediator.Send(new UpdateMarketPricesCommand(session.OwnerId, true, false), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarketWorker: Une erreur est survenue lors de l'exécution de la boucle.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
