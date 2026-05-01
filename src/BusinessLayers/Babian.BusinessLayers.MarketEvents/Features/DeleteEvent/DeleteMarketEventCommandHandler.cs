using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Babian.BusinessLayers.MarketEvents.Features.DeleteEvent;

public class DeleteMarketEventCommandHandler : IRequestHandler<DeleteMarketEventCommand, bool>
{
    private readonly AppDbContext _context;

    public DeleteMarketEventCommandHandler(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<bool> Handle(DeleteMarketEventCommand request, CancellationToken cancellationToken)
    {
        var marketEvent = await _context.MarketEvents
            .FirstOrDefaultAsync(e => e.Id == request.EventId && e.BarmanId == request.BarmanId, cancellationToken);

        if (marketEvent == null)
        {
            return false;
        }

        _context.MarketEvents.Remove(marketEvent);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
