using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using MediatR;

namespace Babian.BusinessLayers.Orders.Features.AddOrder;

public class AddOrderCommandHandler : IRequestHandler<AddOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDrinkRepository _drinkRepository;
    private readonly IMarketSessionRepository _sessionRepository;

    public AddOrderCommandHandler(
        IOrderRepository orderRepository,
        IDrinkRepository drinkRepository,
        IMarketSessionRepository sessionRepository)
    {
        _orderRepository = orderRepository;
        _drinkRepository = drinkRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<Guid> Handle(AddOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Vérifier la session active
        var activeSession = await _sessionRepository.GetActiveSessionAsync(request.OwnerId, cancellationToken);
        if (activeSession == null)
        {
            throw new Exception("Aucune session de marché active pour ce barman.");
        }

        // 2. Récupérer la boisson
        var drinks = await _drinkRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        var drink = drinks.FirstOrDefault(d => d.Id == request.DrinkId);
        
        if (drink == null)
        {
            throw new Exception("Boisson introuvable dans votre catalogue.");
        }

        // 3. Créer la commande
        var order = new Order
        {
            Id = Guid.NewGuid(),
            DrinkId = request.DrinkId,
            Quantity = request.Quantity,
            PriceAtOrder = drink.CurrentPrice,
            MarketSessionId = request.SessionId,
            CycleNumber = request.CycleNumber,
            OwnerId = request.OwnerId,
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepository.AddAsync(order, cancellationToken);
        return order.Id;
    }
}
