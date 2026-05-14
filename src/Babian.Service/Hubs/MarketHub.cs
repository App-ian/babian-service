using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Babian.Service.Hubs;

public class MarketHub : Hub
{
    /// <summary>
    /// Permet au client de rejoindre le flux temps réel d'un bar spécifique.
    /// </summary>
    /// <param name="barmanId">L'identifiant du barman (propriétaire de la session)</param>
    public async Task JoinBar(string barmanId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, barmanId);
    }

    /// <summary>
    /// Permet au client de quitter le flux temps réel.
    /// </summary>
    public async Task LeaveBar(string barmanId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, barmanId);
    }
}
