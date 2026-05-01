using System;
using System.Collections.Generic;
using MediatR;

namespace Babian.BusinessLayers.MarketConfigs.Features.UpdateConfig;

public record RankingGroupDto(string Name, int MaxRank, decimal Coefficient);

public record UpdateMarketConfigCommand(
    Guid BarmanId,
    int CycleDurationSeconds,
    int? TotalCycles,
    decimal DecreaseOthers,
    decimal IncreasePerOrder,
    List<RankingGroupDto> RankingGroups
) : IRequest<Guid>;
