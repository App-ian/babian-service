using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using Babian.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Babian.BusinessLayers.MarketSessions.Features.GetActiveSession;

public class GetActiveSessionQueryHandler : IRequestHandler<GetActiveSessionQuery, MarketSession?>
{
    private readonly AppDbContext _context;

    public GetActiveSessionQueryHandler(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MarketSession?> Handle(GetActiveSessionQuery request, CancellationToken cancellationToken)
    {
        return await _context.MarketSessions
            .Where(s => s.OwnerId == request.OwnerId && s.IsActive)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
