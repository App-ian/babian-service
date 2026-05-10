using FluentValidation;

namespace Babian.BusinessLayers.Orders.Features.AddOrder;

public class AddOrderCommandValidator : AbstractValidator<AddOrderCommand>
{
    public AddOrderCommandValidator()
    {
        RuleFor(x => x.DrinkId).NotEmpty().WithMessage("L'identifiant de la boisson est obligatoire.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("La quantité doit être supérieure à 0.");
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("L'identifiant de la session est obligatoire.");
    }
}
