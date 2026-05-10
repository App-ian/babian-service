using FluentValidation;

namespace Babian.BusinessLayers.MarketEvents.Features.CreateEvent;

public class CreateMarketEventCommandValidator : AbstractValidator<CreateMarketEventCommand>
{
    public CreateMarketEventCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Le nom de l'événement est obligatoire.");
        RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt).WithMessage("La date de fin doit être postérieure à la date de début.");
    }
}
