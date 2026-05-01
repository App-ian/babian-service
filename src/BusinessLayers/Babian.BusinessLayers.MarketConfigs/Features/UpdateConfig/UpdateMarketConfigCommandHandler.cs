using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using MediatR;

namespace Babian.BusinessLayers.MarketConfigs.Features.UpdateConfig;

public class UpdateMarketConfigCommandHandler : IRequestHandler<UpdateMarketConfigCommand, Guid>
{
    private readonly IMarketConfigRepository _repository;

    public UpdateMarketConfigCommandHandler(IMarketConfigRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Guid> Handle(UpdateMarketConfigCommand request, CancellationToken cancellationToken)
    {
        var existingConfig = await _repository.GetByBarmanIdAsync(request.BarmanId, cancellationToken);
        var now = DateTime.UtcNow;

        // Validations
        if (request.RankingGroups == null || !request.RankingGroups.Any())
            throw new Exception("Au moins un groupe de classement est obligatoire.");

        var sortedGroups = request.RankingGroups.OrderBy(g => g.MaxRank).ToList();
        
        // Check for duplicates and strict order
        for (int i = 0; i < sortedGroups.Count; i++)
        {
            if (sortedGroups[i].Coefficient < -1 || sortedGroups[i].Coefficient > 1)
                throw new Exception($"L'intensité du groupe '{sortedGroups[i].Name}' doit être comprise entre -1 et 1.");

            if (i > 0 && sortedGroups[i].MaxRank <= sortedGroups[i - 1].MaxRank)
                throw new Exception($"Les rangs doivent être strictement croissants. Le groupe '{sortedGroups[i].Name}' (Rang {sortedGroups[i].MaxRank}) entre en conflit avec le précédent.");
        }

        // Removed strict 999 check to allow more flexibility as requested by user feedback via front validation

        if (existingConfig == null)
        {
            var newConfig = new MarketConfig
            {
                Id = Guid.NewGuid(),
                BarmanId = request.BarmanId,
                CycleDurationSeconds = request.CycleDurationSeconds,
                TotalCycles = request.TotalCycles,
                DecreaseOthers = request.DecreaseOthers,
                IncreasePerOrder = request.IncreasePerOrder,
                RankingGroups = sortedGroups.Select(g => new RankingGroup
                {
                    Name = g.Name,
                    MaxRank = g.MaxRank,
                    Coefficient = g.Coefficient
                }).ToList(),
                CreatedAt = now,
                UpdatedAt = now
            };
            
            await _repository.AddAsync(newConfig, cancellationToken);
            return newConfig.Id;
        }
        else
        {
            existingConfig.CycleDurationSeconds = request.CycleDurationSeconds;
            existingConfig.TotalCycles = request.TotalCycles;
            existingConfig.DecreaseOthers = request.DecreaseOthers;
            existingConfig.IncreasePerOrder = request.IncreasePerOrder;
            existingConfig.RankingGroups = sortedGroups.Select(g => new RankingGroup
            {
                Name = g.Name,
                MaxRank = g.MaxRank,
                Coefficient = g.Coefficient
            }).ToList();
            existingConfig.UpdatedAt = now;

            await _repository.UpdateAsync(existingConfig, cancellationToken);
            return existingConfig.Id;
        }
    }
}
