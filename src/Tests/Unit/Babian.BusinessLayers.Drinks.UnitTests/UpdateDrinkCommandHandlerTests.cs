using Babian.BusinessLayers.Drinks.Features.UpdateDrink;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Babian.BusinessLayers.Drinks.UnitTests;

public class UpdateDrinkCommandHandlerTests
{
    private readonly Mock<IDrinkRepository> _repoMock = new();
    private readonly UpdateDrinkCommandHandler _sut;

    public UpdateDrinkCommandHandlerTests()
    {
        _sut = new UpdateDrinkCommandHandler(_repoMock.Object);
    }

    private static Drink MakeDrink(Guid drinkId, Guid barmanId) => new()
    {
        Id = drinkId,
        OwnerId = barmanId,
        Name = "Coca-Cola",
        BasePrice = 5m,
        CurrentPrice = 5m,
        MarketPrice = 5m,
        MinPrice = 2m,
        MaxPrice = 10m,
        Category = "Soft",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static UpdateDrinkCommand ValidCommand(Guid drinkId, Guid barmanId) => new(
        DrinkId: drinkId,
        BarmanId: barmanId,
        BasePrice: 5m,
        MinPrice: 2m,
        MaxPrice: 10m,
        ImageUrl: null,
        Category: "Soft",
        Plu: null
    );

    // ─── Not found ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenDrinkNotFound_ReturnsFalse()
    {
        var barmanId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByOwnerIdAsync(barmanId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([]);

        var result = await _sut.Handle(ValidCommand(Guid.NewGuid(), barmanId), CancellationToken.None);

        result.Should().BeFalse();
    }

    // ─── Validations ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenCategoryEmpty_ThrowsException()
    {
        var (drinkId, barmanId) = (Guid.NewGuid(), Guid.NewGuid());
        var drink = MakeDrink(drinkId, barmanId);
        _repoMock.Setup(r => r.GetByOwnerIdAsync(barmanId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([drink]);

        var cmd = ValidCommand(drinkId, barmanId) with { Category = "" };
        var act = () => _sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*catégorie*");
    }

    [Fact]
    public async Task Handle_WhenMinPriceZero_ThrowsException()
    {
        var (drinkId, barmanId) = (Guid.NewGuid(), Guid.NewGuid());
        var drink = MakeDrink(drinkId, barmanId);
        _repoMock.Setup(r => r.GetByOwnerIdAsync(barmanId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([drink]);

        var cmd = ValidCommand(drinkId, barmanId) with { MinPrice = 0m };
        var act = () => _sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*supérieurs à 0*");
    }

    [Fact]
    public async Task Handle_WhenMaxPriceLessThanOrEqualMinPrice_ThrowsException()
    {
        var (drinkId, barmanId) = (Guid.NewGuid(), Guid.NewGuid());
        var drink = MakeDrink(drinkId, barmanId);
        _repoMock.Setup(r => r.GetByOwnerIdAsync(barmanId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([drink]);

        var cmd = ValidCommand(drinkId, barmanId) with { MaxPrice = 2m, MinPrice = 2m };
        var act = () => _sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*maximum doit être supérieur*");
    }

    [Fact]
    public async Task Handle_WhenBasePriceOutsideMinMaxRange_ThrowsException()
    {
        var (drinkId, barmanId) = (Guid.NewGuid(), Guid.NewGuid());
        var drink = MakeDrink(drinkId, barmanId);
        _repoMock.Setup(r => r.GetByOwnerIdAsync(barmanId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([drink]);

        var cmd = ValidCommand(drinkId, barmanId) with { BasePrice = 1m, MinPrice = 2m, MaxPrice = 10m };
        var act = () => _sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*compris entre*");
    }

    [Fact]
    public async Task Handle_WhenMaxPriceAbove200_ThrowsException()
    {
        var (drinkId, barmanId) = (Guid.NewGuid(), Guid.NewGuid());
        var drink = MakeDrink(drinkId, barmanId);
        _repoMock.Setup(r => r.GetByOwnerIdAsync(barmanId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([drink]);

        var cmd = ValidCommand(drinkId, barmanId) with { BasePrice = 150m, MinPrice = 1m, MaxPrice = 250m };
        var act = () => _sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*200€*");
    }

    [Fact]
    public async Task Handle_WhenPluAlreadyUsedByAnotherDrink_ThrowsException()
    {
        var (drinkId, barmanId) = (Guid.NewGuid(), Guid.NewGuid());
        var drink = MakeDrink(drinkId, barmanId);
        var otherDrink = MakeDrink(Guid.NewGuid(), barmanId);
        otherDrink.Plu = "PLU001";

        _repoMock.Setup(r => r.GetByOwnerIdAsync(barmanId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([drink, otherDrink]);

        var cmd = ValidCommand(drinkId, barmanId) with { Plu = "PLU001" };
        var act = () => _sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*PLU*déjà utilisé*");
    }

    // ─── Succès ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenValid_UpdatesDrinkAndReturnsTrue()
    {
        var (drinkId, barmanId) = (Guid.NewGuid(), Guid.NewGuid());
        var drink = MakeDrink(drinkId, barmanId);
        _repoMock.Setup(r => r.GetByOwnerIdAsync(barmanId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([drink]);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Drink>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var cmd = ValidCommand(drinkId, barmanId) with { Category = "Bière", BasePrice = 6m, MinPrice = 3m, MaxPrice = 12m };
        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.UpdateAsync(It.Is<Drink>(d => d.Category == "Bière" && d.BasePrice == 6m), It.IsAny<CancellationToken>()), Times.Once);
    }
}
