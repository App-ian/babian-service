using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Babian.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetQuantityByDrinkAndSessionAsync(Guid drinkId, Guid sessionId, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Where(x => x.DrinkId == drinkId && x.MarketSessionId == sessionId)
            .SumAsync(x => x.Quantity, cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetCountsBySessionAsync(Guid sessionId, int? cycleNumber, DateTime? since, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Where(x => x.MarketSessionId == sessionId);

        if (cycleNumber.HasValue)
        {
            query = query.Where(x => x.CycleNumber == cycleNumber.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= since.Value);
        }

        var results = await query
            .GroupBy(x => x.DrinkId)
            .Select(g => new { DrinkId = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToListAsync(cancellationToken);

        return results.ToDictionary(x => x.DrinkId, x => x.Total);
    }
}
