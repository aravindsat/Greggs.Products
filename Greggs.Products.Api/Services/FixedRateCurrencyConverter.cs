using System;

namespace Greggs.Products.Api.Services;

public class FixedRateCurrencyConverter : ICurrencyConverter
{
    // Given exchange rate of 1 GBP to 1.11 EUR
    private const decimal GbpToEurRate = 1.11m;

    public decimal ConvertGbpToEur(decimal pounds)
    {
        // 2dp is typical for currency display
        return decimal.Round(pounds * GbpToEurRate, 2, MidpointRounding.AwayFromZero);
    }
}
