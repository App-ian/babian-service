using FluentValidation;

namespace Babian.BusinessLayers.Drinks.Features.UpdateDrink;

public class UpdateDrinkCommandValidator : AbstractValidator<UpdateDrinkCommand>
{
    public UpdateDrinkCommandValidator()
    {
        RuleFor(x => x.DrinkId).NotEmpty().WithMessage("L'identifiant de la boisson est obligatoire.");
        
        RuleFor(x => x.MinPrice).GreaterThan(0).WithMessage("Le prix minimum doit être supérieur à 0.");
        RuleFor(x => x.BasePrice).GreaterThan(0).WithMessage("Le prix de base doit être supérieur à 0.");
        RuleFor(x => x.MaxPrice).GreaterThan(0).WithMessage("Le prix maximum doit être supérieur à 0.");
        
        RuleFor(x => x.MaxPrice)
            .GreaterThan(x => x.MinPrice).WithMessage("Le prix maximum doit être supérieur au prix minimum.");
            
        RuleFor(x => x.BasePrice)
            .Must((cmd, basePrice) => basePrice >= cmd.MinPrice && basePrice <= cmd.MaxPrice)
            .WithMessage("Le prix de base doit être compris entre le prix minimum et le prix maximum.");
            
        RuleFor(x => x.MaxPrice)
            .LessThanOrEqualTo(200).WithMessage("Le prix maximum ne peut pas dépasser 200€ (seuil de sécurité).");
    }
}
