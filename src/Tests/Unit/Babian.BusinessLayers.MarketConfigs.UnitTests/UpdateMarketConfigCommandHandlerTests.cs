using Babian.BusinessLayers.MarketConfigs.Features.UpdateConfig;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Babian.BusinessLayers.MarketConfigs.UnitTests;

public class UpdateMarketConfigCommandHandlerTests
{
    private readonly Mock<IMarketConfigRepository> _repoMock = new();
    private readonly UpdateMarketConfigCommandHandler _sut;

    public UpdateMarketConfigCommandHandlerTests()
    {
        _sut = new UpdateMarketConfigCommandHandler(_repoMock.Object);
    }

    private static UpdateMarketConfigCommand ValidCommand(Guid barmanId) => new(
        BarmanId: barmanId,
        CycleDurationSeconds: 30,
        DecreaseOthers: 0.05m,
        IncreasePerOrder: 0.10m,
        RankingGroups: new List<RankingGroupDto>
        {
            new("Top", 10, 0.3m),
            new("Bottom", 99, -0.2m)
        }
    );

    // ─── Validations ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenRankingGroupsNull_ThrowsException()
    {
        var cmd = ValidCommand(Guid.NewGuid()) with { RankingGroups = null! };
        var act = () => _sut.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*obligatoire*");
    }

    [Fact]
    public async Task Handle_WhenRankingGroupsEmpty_ThrowsException()
    {
        var cmd = ValidCommand(Guid.NewGuid()) with { RankingGroups = [] };
        var act = () => _sut.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*obligatoire*");
    }

    [Fact]
    public async Task Handle_WhenCoefficientOutOfRange_ThrowsException()
    {
        var cmd = ValidCommand(Guid.NewGuid()) with
        {
            RankingGroups = [new RankingGroupDto("Bad", 10, 1.5m)]
        };
        var act = () => _sut.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*intensité*");
    }

    [Fact]
    public async Task Handle_WhenRanksNotStrictlyIncreasing_ThrowsException()
    {
        var cmd = ValidCommand(Guid.NewGuid()) with
        {
            RankingGroups =
            [
                new RankingGroupDto("A", 20, 0.2m),
                new RankingGroupDto("B", 10, -0.1m)  // MaxRank inférieur → conflit après tri
            ]
        };
        // Après tri par MaxRank : [B=10, A=20] — B.MaxRank < A.MaxRank, OK
        // Mais si on met deux fois le même rang : conflit
        var cmdDuplicate = ValidCommand(Guid.NewGuid()) with
        {
            RankingGroups =
            [
                new RankingGroupDto("A", 10, 0.2m),
                new RankingGroupDto("B", 10, -0.1m)  // même MaxRank → conflit
            ]
        };
        var act = () => _sut.Handle(cmdDuplicate, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*strictement croissants*");
    }

    // ─── Création (config inexistante) ───────────────────────────────────────

    [Fact]
    public async Task Handle_WhenNoExistingConfig_CallsAddAsync_AndReturnsNewGuid()
    {
        // Arrange
        var barmanId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByBarmanIdAsync(barmanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketConfig?)null);
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<MarketConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(ValidCommand(barmanId), CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<MarketConfig>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<MarketConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─── Mise à jour (config existante) ─────────────────────────────────────

    [Fact]
    public async Task Handle_WhenConfigExists_CallsUpdateAsync_AndReturnsExistingId()
    {
        // Arrange
        var barmanId = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var existing = new MarketConfig { Id = existingId, BarmanId = barmanId, RankingGroups = [] };

        _repoMock
            .Setup(r => r.GetByBarmanIdAsync(barmanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<MarketConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(ValidCommand(barmanId), CancellationToken.None);

        // Assert
        result.Should().Be(existingId);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<MarketConfig>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<MarketConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RankingGroupsSortedByMaxRank_InStoredConfig()
    {
        // Arrange — groupes envoyés dans l'ordre inverse
        var barmanId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByBarmanIdAsync(barmanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketConfig?)null);

        MarketConfig? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<MarketConfig>(), It.IsAny<CancellationToken>()))
            .Callback<MarketConfig, CancellationToken>((cfg, _) => captured = cfg)
            .Returns(Task.CompletedTask);

        var cmd = ValidCommand(barmanId) with
        {
            RankingGroups =
            [
                new RankingGroupDto("Bottom", 99, -0.2m),
                new RankingGroupDto("Top", 10, 0.3m)
            ]
        };

        // Act
        await _sut.Handle(cmd, CancellationToken.None);

        // Assert — les groupes doivent être triés par MaxRank croissant
        captured!.RankingGroups.First().MaxRank.Should().Be(10);
        captured.RankingGroups.Last().MaxRank.Should().Be(99);
    }
}
