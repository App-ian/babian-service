using FluentValidation;

namespace Babian.BusinessLayers.MarketConfigs.Features.UpdateConfig;

public class UpdateMarketConfigValidator : AbstractValidator<UpdateMarketConfigCommand>
{
    public UpdateMarketConfigValidator()
    {
        RuleFor(x => x.BarmanId)
            .NotEmpty().WithMessage("Le BarmanId est requis.");

        RuleFor(x => x.CycleDurationSeconds)
            .GreaterThanOrEqualTo(10).WithMessage("La durée du cycle doit être d'au moins 10 secondes.");

        RuleFor(x => x.DecreaseOthers)
            .InclusiveBetween(0, 1).WithMessage("Le coefficient de baisse doit être entre 0 et 1.");

        RuleFor(x => x.IncreasePerOrder)
            .InclusiveBetween(0, 1).WithMessage("Le coefficient de hausse doit être entre 0 et 1.");

        RuleFor(x => x.RankingGroups)
            .NotNull()
            .Must(x => x == null || x.Count > 0).WithMessage("Au moins un groupe de classement doit être défini.");

        RuleForEach(x => x.RankingGroups).ChildRules(group =>
        {
            group.RuleFor(g => g.Name).NotEmpty().WithMessage("Le nom du groupe est requis.");
            group.RuleFor(g => g.MaxRank).GreaterThan(0).WithMessage("Le rang maximum doit être supérieur à 0.");
            group.RuleFor(g => g.Coefficient).InclusiveBetween(-1, 1).WithMessage("Le coefficient doit être compris entre -1 et 1.");
        });
    }
}
