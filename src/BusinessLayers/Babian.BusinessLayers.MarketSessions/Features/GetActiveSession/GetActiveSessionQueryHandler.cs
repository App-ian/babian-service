using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Babian.BusinessLayers.MarketSessions.Features.GetActiveSession;

public class GetActiveSessionQueryHandler : IRequestHandler<GetActiveSessionQuery, MarketSessionDto?>
{
    private readonly AppDbContext _context;

    public GetActiveSessionQueryHandler(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MarketSessionDto?> Handle(GetActiveSessionQuery request, CancellationToken cancellationToken)
    {
        return await _context.MarketSessions
            .Where(s => s.OwnerId == request.OwnerId && s.IsActive)
            .OrderByDescending(s => s.StartTime)
            .Select(s => new MarketSessionDto(
                s.Id,
                s.Name,
                s.StartTime,
                s.EndTime,
                s.IsActive,
                s.CreatedAt,
                s.CurrentCycleNumber,
                s.LastPriceUpdateAt,
                s.OwnerId
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
