using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using MediatR;

namespace Babian.BusinessLayers.GlobalDrinks.Features.DeleteGlobalDrink;

public record DeleteGlobalDrinkCommand(Guid Id) : IRequest;

public class DeleteGlobalDrinkCommandHandler : IRequestHandler<DeleteGlobalDrinkCommand>
{
    private readonly IGlobalDrinkRepository _repository;

    public DeleteGlobalDrinkCommandHandler(IGlobalDrinkRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteGlobalDrinkCommand request, CancellationToken cancellationToken)
    {
        var globalDrink = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (globalDrink == null)
        {
            throw new Exception($"GlobalDrink with ID {request.Id} not found.");
        }

        await _repository.DeleteAsync(globalDrink, cancellationToken);
    }
}
