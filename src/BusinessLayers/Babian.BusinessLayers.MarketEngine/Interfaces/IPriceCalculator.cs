namespace Babian.BusinessLayers.MarketEngine.Interfaces;

public interface IPriceCalculator
{
    /// <summary>
    /// Calcule le nouveau prix d'une boisson selon l'algorithme asymptotique.
    /// </summary>
    /// <param name="currentPrice">Prix actuel</param>
    /// <param name="minPrice">Prix minimum autorisé</param>
    /// <param name="maxPrice">Prix maximum autorisé</param>
    /// <param name="targetPrice">Prix cible (basé sur la demande)</param>
    /// <param name="speed">Vitesse de variation (0 à 1)</param>
    /// <returns>Le nouveau prix calculé</returns>
    decimal CalculateNextPrice(decimal currentPrice, decimal minPrice, decimal maxPrice, decimal targetPrice, decimal speed);
}
