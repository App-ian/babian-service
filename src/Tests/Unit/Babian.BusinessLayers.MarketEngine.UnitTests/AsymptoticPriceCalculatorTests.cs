using Babian.BusinessLayers.MarketEngine.Services;
using Babian.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Babian.BusinessLayers.MarketEngine.UnitTests;

public class AsymptoticPriceCalculatorTests
{
    private readonly AsymptoticPriceCalculator _sut = new();

    [Fact]
    public void CalculateNextPrice_WhenCoefficientPositive_PriceMoveTowardMax()
    {
        // Arrange
        decimal current = 5m, min = 2m, max = 10m, target = 10m, speed = 0.2m;

        // Act
        decimal result = _sut.CalculateNextPrice(current, min, max, target, speed);

        // Assert
        result.Should().BeGreaterThan(current);
        result.Should().BeLessThanOrEqualTo(max);
    }

    [Fact]
    public void CalculateNextPrice_WhenCoefficientNegative_PriceMoveTowardMin()
    {
        // Arrange
        decimal current = 8m, min = 2m, max = 10m, target = 2m, speed = 0.2m;

        // Act
        decimal result = _sut.CalculateNextPrice(current, min, max, target, speed);

        // Assert
        result.Should().BeLessThan(current);
        result.Should().BeGreaterThanOrEqualTo(min);
    }

    [Fact]
    public void CalculateNextPrice_ClampsBelowMin()
    {
        // Arrange — speed élevée pour forcer dépassement
        decimal current = 2.1m, min = 2m, max = 10m, target = 2m, speed = 1m;

        // Act
        decimal result = _sut.CalculateNextPrice(current, min, max, target, speed);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(min);
    }

    [Fact]
    public void CalculateNextPrice_ClampsAboveMax()
    {
        // Arrange
        decimal current = 9.9m, min = 2m, max = 10m, target = 10m, speed = 1m;

        // Act
        decimal result = _sut.CalculateNextPrice(current, min, max, target, speed);

        // Assert
        result.Should().BeLessThanOrEqualTo(max);
    }

    [Fact]
    public void CalculateNextPrice_RoundsToTwoDecimals()
    {
        // Arrange
        decimal current = 5.555m, min = 2m, max = 10m, target = 10m, speed = 0.333m;

        // Act
        decimal result = _sut.CalculateNextPrice(current, min, max, target, speed);

        // Assert
        var decimals = BitConverter.GetBytes(decimal.GetBits(result)[3])[2];
        decimals.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public void CalculateNextPrice_WhenAtBoundary_ReturnsExactBoundary()
    {
        // Arrange — déjà au max
        decimal current = 10m, min = 2m, max = 10m, target = 10m, speed = 0.5m;

        // Act
        decimal result = _sut.CalculateNextPrice(current, min, max, target, speed);

        // Assert
        result.Should().Be(max);
    }
}
