using Babian.Domain.Interfaces;
using Babian.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Babian.Service.Services;

public class MarketNotificationService : IMarketNotificationService
{
    private readonly IHubContext<MarketHub> _hubContext;

    public MarketNotificationService(IHubContext<MarketHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyPricesUpdatedAsync(Guid barmanId, object prices)
    {
        await _hubContext.Clients.Group(barmanId.ToString()).SendAsync("ReceivePriceUpdate", prices);
    }

    public async Task NotifyEventAsync(Guid barmanId, string message, object? data = null)
    {
        await _hubContext.Clients.Group(barmanId.ToString()).SendAsync("ReceiveMarketEvent", new { message, data });
    }
}
