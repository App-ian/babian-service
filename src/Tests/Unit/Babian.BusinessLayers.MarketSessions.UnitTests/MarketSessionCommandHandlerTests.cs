using Babian.BusinessLayers.MarketSessions.Features.StartSession;
using Babian.BusinessLayers.MarketSessions.Features.StopSession;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Babian.BusinessLayers.MarketSessions.UnitTests;

public class StartMarketSessionCommandHandlerTests
{
    private readonly Mock<IMarketSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IDrinkRepository> _drinkRepoMock = new();
    private readonly StartMarketSessionCommandHandler _sut;

    public StartMarketSessionCommandHandlerTests()
    {
        _sut = new StartMarketSessionCommandHandler(_sessionRepoMock.Object, _drinkRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSessionAlreadyActive_ReturnsExistingId()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var existingSession = new MarketSession { Id = Guid.NewGuid(), OwnerId = ownerId, IsActive = true };
        _sessionRepoMock
            .Setup(r => r.GetActiveSessionAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSession);

        // Act
        var result = await _sut.Handle(new StartMarketSessionCommand(ownerId, "Test"), CancellationToken.None);

        // Assert
        result.Should().Be(existingSession.Id);
        _sessionRepoMock.Verify(r => r.CreateAsync(It.IsAny<MarketSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoActiveSession_CreatesNewSession()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _sessionRepoMock
            .Setup(r => r.GetActiveSessionAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketSession?)null);
        _drinkRepoMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _sessionRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<MarketSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketSession s, CancellationToken _) => s);

        // Act
        var result = await _sut.Handle(new StartMarketSessionCommand(ownerId, "Soirée"), CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _sessionRepoMock.Verify(r => r.CreateAsync(
            It.Is<MarketSession>(s => s.IsActive == true && s.CurrentCycleNumber == 1 && s.OwnerId == ownerId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDrinkCurrentPriceDiffersFromBase_ResetsToBasePrice()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var drink = new Drink { Id = Guid.NewGuid(), OwnerId = ownerId, BasePrice = 5m, CurrentPrice = 8m, IsActive = true };

        _sessionRepoMock
            .Setup(r => r.GetActiveSessionAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketSession?)null);
        _drinkRepoMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([drink]);
        _sessionRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<MarketSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketSession s, CancellationToken _) => s);
        _drinkRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Drink>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(new StartMarketSessionCommand(ownerId, "Soirée"), CancellationToken.None);

        // Assert — UpdateAsync appelé car CurrentPrice ≠ BasePrice
        _drinkRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Drink>(d => d.CurrentPrice == 5m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDrinkCurrentPriceEqualsBase_DoesNotCallUpdate()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var drink = new Drink { Id = Guid.NewGuid(), OwnerId = ownerId, BasePrice = 5m, CurrentPrice = 5m, IsActive = true };

        _sessionRepoMock
            .Setup(r => r.GetActiveSessionAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketSession?)null);
        _drinkRepoMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([drink]);
        _sessionRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<MarketSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketSession s, CancellationToken _) => s);

        // Act
        await _sut.Handle(new StartMarketSessionCommand(ownerId, "Soirée"), CancellationToken.None);

        // Assert — UpdateAsync PAS appelé car prix déjà au BasePrice
        _drinkRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Drink>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class StopMarketSessionCommandHandlerTests
{
    private readonly Mock<IMarketSessionRepository> _sessionRepoMock = new();
    private readonly StopMarketSessionCommandHandler _sut;

    public StopMarketSessionCommandHandlerTests()
    {
        _sut = new StopMarketSessionCommandHandler(_sessionRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNoActiveSession_ReturnsFalse()
    {
        var ownerId = Guid.NewGuid();
        _sessionRepoMock
            .Setup(r => r.GetActiveSessionAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketSession?)null);

        var result = await _sut.Handle(new StopMarketSessionCommand(ownerId), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenActiveSession_SetsInactiveAndReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var session = new MarketSession { Id = Guid.NewGuid(), OwnerId = ownerId, IsActive = true };

        _sessionRepoMock
            .Setup(r => r.GetActiveSessionAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _sessionRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<MarketSession>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(new StopMarketSessionCommand(ownerId), CancellationToken.None);

        result.Should().BeTrue();
        _sessionRepoMock.Verify(r => r.UpdateAsync(
            It.Is<MarketSession>(s => s.IsActive == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
