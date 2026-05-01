namespace Babian.Domain.Entities;

public class RankingGroup
{
    public string Name { get; set; } = string.Empty;
    public int MaxRank { get; set; }
    public decimal Coefficient { get; set; }
}
