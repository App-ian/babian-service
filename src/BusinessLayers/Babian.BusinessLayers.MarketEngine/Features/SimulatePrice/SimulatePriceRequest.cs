using System.Collections.Generic;

namespace Babian.BusinessLayers.MarketEngine.Features.SimulatePrice;

public class SimulatePriceRequest
{
    public decimal CurrentPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int Rank { get; set; }
    public List<SimulatedRankingGroupDto> Groups { get; set; }
}

public class SimulatedRankingGroupDto
{
    public string Name { get; set; }
    public int MaxRank { get; set; }
    public decimal Coefficient { get; set; }
}
