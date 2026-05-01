using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.BusinessLayers.Orders.Features.AddOrder;
using Babian.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Babian.BusinessLayers.Orders.Features.ProcessPosSales;

public class ProcessPosSalesCommandHandler : IRequestHandler<ProcessPosSalesCommand, bool>
{
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;

    public ProcessPosSalesCommandHandler(AppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<bool> Handle(ProcessPosSalesCommand request, CancellationToken cancellationToken)
    {
        // Find Drink
        var drink = await _context.Drinks
            .FirstOrDefaultAsync(d => d.OwnerId == request.OwnerId && d.Plu == request.Plu, cancellationToken);
            
        if (drink == null)
        {
            throw new Exception($"Cet article avec le PLU {request.Plu} n'est pas assigné dans le catalogue boursier.");
        }

        // Find Active Session
        var activeSession = await _context.MarketSessions
            .Where(s => s.OwnerId == request.OwnerId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSession == null)
        {
            throw new Exception("Aucune session de marché boursier active actuellement pour ce restaurant.");
        }

        // Send AddOrderCommand
        var addOrderCommand = new AddOrderCommand(
            request.OwnerId,
            drink.Id,
            request.Quantity,
            activeSession.Id,
            activeSession.CurrentCycleNumber
        );

        await _mediator.Send(addOrderCommand, cancellationToken);

        return true;
    }
}
