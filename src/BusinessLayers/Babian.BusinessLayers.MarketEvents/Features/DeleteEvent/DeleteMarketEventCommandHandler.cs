using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Exceptions;
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
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

        if (marketEvent == null)
            throw new NotFoundException($"Événement {request.EventId} introuvable.");

        if (marketEvent.BarmanId != request.BarmanId)
            throw new ForbiddenException("Vous n'êtes pas propriétaire de cet événement.");

        _context.MarketEvents.Remove(marketEvent);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
