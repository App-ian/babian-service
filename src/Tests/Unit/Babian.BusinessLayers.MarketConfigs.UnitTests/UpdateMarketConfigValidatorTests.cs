using Babian.BusinessLayers.MarketConfigs.Features.UpdateConfig;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;

namespace Babian.BusinessLayers.MarketConfigs.UnitTests;

public class UpdateMarketConfigValidatorTests
{
    private readonly UpdateMarketConfigValidator _validator = new();

    private static UpdateMarketConfigCommand ValidCommand() => new(
        BarmanId: Guid.NewGuid(),
        CycleDurationSeconds: 30,
        DecreaseOthers: 0.05m,
        IncreasePerOrder: 0.10m,
        RankingGroups: [new RankingGroupDto("Top", 10, 0.3m)]
    );

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyBarmanId_HasError()
    {
        var cmd = ValidCommand() with { BarmanId = Guid.Empty };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.BarmanId);
    }

    [Fact]
    public void Validate_CycleDurationBelowMin_HasError()
    {
        var cmd = ValidCommand() with { CycleDurationSeconds = 5 };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.CycleDurationSeconds);
    }

    [Fact]
    public void Validate_DecreaseOthersAboveOne_HasError()
    {
        var cmd = ValidCommand() with { DecreaseOthers = 1.5m };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.DecreaseOthers);
    }

    [Fact]
    public void Validate_DecreaseOthersNegative_HasError()
    {
        var cmd = ValidCommand() with { DecreaseOthers = -0.1m };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.DecreaseOthers);
    }

    [Fact]
    public void Validate_IncreasePerOrderAboveOne_HasError()
    {
        var cmd = ValidCommand() with { IncreasePerOrder = 2m };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.IncreasePerOrder);
    }

    [Fact]
    public void Validate_RankingGroupsNull_HasError()
    {
        var cmd = ValidCommand() with { RankingGroups = null! };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.RankingGroups);
    }

    [Fact]
    public void Validate_GroupWithEmptyName_HasError()
    {
        var cmd = ValidCommand() with
        {
            RankingGroups = [new RankingGroupDto("", 10, 0.2m)]
        };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor("RankingGroups[0].Name");
    }

    [Fact]
    public void Validate_GroupWithMaxRankZero_HasError()
    {
        var cmd = ValidCommand() with
        {
            RankingGroups = [new RankingGroupDto("Bad", 0, 0.2m)]
        };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor("RankingGroups[0].MaxRank");
    }

    [Fact]
    public void Validate_GroupCoefficientAboveOne_HasError()
    {
        var cmd = ValidCommand() with
        {
            RankingGroups = [new RankingGroupDto("Bad", 10, 1.5m)]
        };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor("RankingGroups[0].Coefficient");
    }

    [Fact]
    public void Validate_GroupCoefficientBelowMinusOne_HasError()
    {
        var cmd = ValidCommand() with
        {
            RankingGroups = [new RankingGroupDto("Bad", 10, -1.1m)]
        };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor("RankingGroups[0].Coefficient");
    }
}
