using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.BusinessLayers.GlobalDrinks.Features.AddGlobalDrink;
using Babian.BusinessLayers.GlobalDrinks.Features.DeleteGlobalDrink;
using Babian.BusinessLayers.GlobalDrinks.Features.UpdateGlobalDrink;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Babian.BusinessLayers.GlobalDrinks.UnitTests;

public class GlobalDrinksCommandsTests
{
    private readonly Mock<IGlobalDrinkRepository> _repositoryMock;

    public GlobalDrinksCommandsTests()
    {
        _repositoryMock = new Mock<IGlobalDrinkRepository>();
    }

    [Fact]
    public async Task AddGlobalDrink_ShouldReturnGuidAndCallRepository()
    {
        // Arrange
        var handler = new AddGlobalDrinkCommandHandler(_repositoryMock.Object);
        var command = new AddGlobalDrinkCommand("Nouvelle Boisson", 5.5m, null, "33cl", "Bières", "123", true);

        GlobalDrink? addedDrink = null;
        _repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<GlobalDrink>(), It.IsAny<CancellationToken>()))
            .Callback<GlobalDrink, CancellationToken>((drink, ct) => {
                drink.Id = Guid.NewGuid();
                addedDrink = drink;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        addedDrink.Should().NotBeNull();
        addedDrink!.Name.Should().Be("Nouvelle Boisson");
        addedDrink.DefaultBasePrice.Should().Be(5.5m);
        _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<GlobalDrink>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGlobalDrink_ShouldUpdatePropertiesAndCallRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingDrink = new GlobalDrink { Id = id, Name = "Ancienne Boisson", DefaultBasePrice = 4m };
        
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDrink);

        var handler = new UpdateGlobalDrinkCommandHandler(_repositoryMock.Object);
        var command = new UpdateGlobalDrinkCommand(id, "Boisson Modifiée", 6m, null, null, null, null, true);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        existingDrink.Name.Should().Be("Boisson Modifiée");
        existingDrink.DefaultBasePrice.Should().Be(6m);
        _repositoryMock.Verify(repo => repo.UpdateAsync(existingDrink, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteGlobalDrink_ShouldCallDeleteOnRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingDrink = new GlobalDrink { Id = id, Name = "Boisson à supprimer" };
        
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDrink);

        var handler = new DeleteGlobalDrinkCommandHandler(_repositoryMock.Object);
        var command = new DeleteGlobalDrinkCommand(id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(repo => repo.DeleteAsync(existingDrink, It.IsAny<CancellationToken>()), Times.Once);
    }
}
