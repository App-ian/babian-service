using Babian.BusinessLayers.MarketEngine.Services;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Babian.BusinessLayers.MarketEngine.UnitTests;

public class MarketSimulationServiceTests
{
    private readonly Mock<IPriceCalculator> _calculatorMock = new();
    private readonly MarketSimulationService _sut;

    public MarketSimulationServiceTests()
    {
        _sut = new MarketSimulationService(_calculatorMock.Object);
    }

    [Fact]
    public void SimulateNextPrice_WithRankingGroups_UsesCorrectGroup()
    {
        // Arrange
        var groups = new List<RankingGroup>
        {
            new() { Name = "Top", MaxRank = 10, Coefficient = 0.3m },
            new() { Name = "Mid", MaxRank = 20, Coefficient = -0.1m }
        };
        _calculatorMock
            .Setup(c => c.CalculateNextPrice(5m, 2m, 10m, 10m, 0.3m))
            .Returns(6.5m);

        // Act — rank 5, dans le groupe Top (MaxRank=10), coefficient=0.3 → target=max, speed=0.3
        var result = _sut.SimulateNextPrice(5m, 2m, 10m, rank: 5, groups);

        // Assert
        _calculatorMock.Verify(c => c.CalculateNextPrice(5m, 2m, 10m, 10m, 0.3m), Times.Once);
        result.Should().Be(6.5m);
    }

    [Fact]
    public void SimulateNextPrice_RankBeyondAllGroups_FallsBackToLastGroup()
    {
        // Arrange
        var groups = new List<RankingGroup>
        {
            new() { Name = "Top", MaxRank = 5, Coefficient = 0.2m },
            new() { Name = "Bottom", MaxRank = 10, Coefficient = -0.2m }
        };
        _calculatorMock
            .Setup(c => c.CalculateNextPrice(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), 0.2m))
            .Returns(4m);

        // Act — rank 99 → fallback sur le dernier groupe (Bottom, coefficient=-0.2)
        _sut.SimulateNextPrice(8m, 2m, 10m, rank: 99, groups);

        // Assert : target=min=2m, speed=0.2
        _calculatorMock.Verify(c => c.CalculateNextPrice(8m, 2m, 10m, 2m, 0.2m), Times.Once);
    }

    [Fact]
    public void SimulateNextPrice_NullGroups_UsesBinaryFallback_HighRankDecreases()
    {
        // Arrange
        _calculatorMock
            .Setup(c => c.CalculateNextPrice(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(5m);

        // Act — rank 20 > 15 → coefficient négatif → target=min, speed=0.15
        _sut.SimulateNextPrice(8m, 2m, 10m, rank: 20, groups: null);

        // Assert
        _calculatorMock.Verify(c => c.CalculateNextPrice(8m, 2m, 10m, 2m, 0.15m), Times.Once);
    }

    [Fact]
    public void SimulateNextPrice_NullGroups_UsesBinaryFallback_LowRankIncreases()
    {
        // Arrange
        _calculatorMock
            .Setup(c => c.CalculateNextPrice(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(7m);

        // Act — rank 10 ≤ 15 → coefficient positif → target=max, speed=0.15
        _sut.SimulateNextPrice(5m, 2m, 10m, rank: 10, groups: null);

        // Assert
        _calculatorMock.Verify(c => c.CalculateNextPrice(5m, 2m, 10m, 10m, 0.15m), Times.Once);
    }

    [Fact]
    public void SimulateNextPrice_CoefficientZero_ReturnsPriceUnchanged()
    {
        // Arrange
        var groups = new List<RankingGroup>
        {
            new() { Name = "Stable", MaxRank = 99, Coefficient = 0m }
        };

        // Act — coefficient 0 → prix stable, calculateur non appelé
        var result = _sut.SimulateNextPrice(5m, 2m, 10m, rank: 1, groups);

        // Assert
        result.Should().Be(5m);
        _calculatorMock.Verify(c => c.CalculateNextPrice(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
    }
}
