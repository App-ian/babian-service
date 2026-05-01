namespace Babian.Domain.Interfaces;

public interface IPriceCalculator
{
    /// <summary>
    /// Calcule le nouveau prix d'une boisson selon l'algorithme asymptotique.
    /// </summary>
    decimal CalculateNextPrice(decimal currentPrice, decimal minPrice, decimal maxPrice, decimal targetPrice, decimal speed);
}
