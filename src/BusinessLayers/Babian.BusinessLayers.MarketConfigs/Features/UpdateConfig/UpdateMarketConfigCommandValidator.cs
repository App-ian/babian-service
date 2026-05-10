using FluentValidation;
using System.Linq;

namespace Babian.BusinessLayers.MarketConfigs.Features.UpdateConfig;

public class UpdateMarketConfigCommandValidator : AbstractValidator<UpdateMarketConfigCommand>
{
    public UpdateMarketConfigCommandValidator()
    {
        RuleFor(x => x.CycleDurationSeconds).GreaterThan(0).WithMessage("La durée du cycle doit être supérieure à 0.");
        
        RuleFor(x => x.RankingGroups)
            .NotEmpty().WithMessage("Au moins un groupe de classement est obligatoire.");

        RuleForEach(x => x.RankingGroups).ChildRules(group => {
            group.RuleFor(g => g.Coefficient)
                .InclusiveBetween(-1, 1)
                .WithMessage(g => $"L'intensité du groupe '{g.Name}' doit être comprise entre -1 et 1.");
        });

        RuleFor(x => x.RankingGroups)
            .Must(groups => {
                if (groups == null || groups.Count <= 1) return true;
                var sorted = groups.OrderBy(g => g.MaxRank).ToList();
                for (int i = 1; i < sorted.Count; i++)
                {
                    if (sorted[i].MaxRank <= sorted[i - 1].MaxRank) return false;
                }
                return true;
            })
            .WithMessage("Les rangs doivent être strictement croissants.");
    }
}
