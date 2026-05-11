using System;
using System.Threading.Tasks;

namespace Babian.Domain.Interfaces;

public interface IMarketNotificationService
{
    Task NotifyPricesUpdatedAsync(Guid barmanId, object prices);
    Task NotifyEventAsync(Guid barmanId, string message, object? data = null);
}
