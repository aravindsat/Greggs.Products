using Greggs.Products.Api.Options;
using Greggs.Products.Api.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace Greggs.Products.UnitTests;

public class FixedRateCurrencyConverterTests
{
    private static ICurrencyConverter CreateConverter(decimal rate = 1.11m)
    {
        return new FixedRateCurrencyConverter(Options.Create(new CurrencyOptions { GbpToEurRate = rate }));
    }

    [Theory]
    [InlineData(1.00, 1.11)]
    [InlineData(0.70, 0.78)]
    [InlineData(1.20, 1.33)]
    [InlineData(1.95, 2.16)]
    [InlineData(2.10, 2.33)]
    public void ConvertGbpToEur_ReturnsCorrectlyRoundedValue(decimal gbp, decimal expectedEur)
    {
        var converter = CreateConverter();

        Assert.Equal(expectedEur, converter.ConvertGbpToEur(gbp));
    }

    [Fact]
    public void ConvertGbpToEur_ZeroAmount_ReturnsZero()
    {
        var converter = CreateConverter();

        Assert.Equal(0.00m, converter.ConvertGbpToEur(0.00m));
    }

    [Fact]
    public void ConvertGbpToEur_UsesRateFromOptions()
    {
        var converter = CreateConverter(rate: 1.20m);

        Assert.Equal(1.20m, converter.ConvertGbpToEur(1.00m));
    }

    [Fact]
    public void ConvertGbpToEur_RoundsToTwoDecimalPlaces()
    {
        var converter = CreateConverter(rate: 1.11m);

        // 0.5 * 1.11 = 0.555 — rounds up to 0.56 (AwayFromZero)
        Assert.Equal(0.56m, converter.ConvertGbpToEur(0.5m));
    }
}
