using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using Babian.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Babian.BusinessLayers.MarketConfigs.Features.GetConfig;

public class GetMarketConfigQueryHandler : IRequestHandler<GetMarketConfigQuery, MarketConfig?>
{
    private readonly AppDbContext _context;

    public GetMarketConfigQueryHandler(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MarketConfig?> Handle(GetMarketConfigQuery request, CancellationToken cancellationToken)
    {
        return await _context.MarketConfigs
            .FirstOrDefaultAsync(c => c.BarmanId == request.OwnerId, cancellationToken);
    }
}
