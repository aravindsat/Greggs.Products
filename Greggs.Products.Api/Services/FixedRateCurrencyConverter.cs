using System;
using Greggs.Products.Api.Options;
using Microsoft.Extensions.Options;

namespace Greggs.Products.Api.Services;

public class FixedRateCurrencyConverter : ICurrencyConverter
{
    private readonly decimal _gbpToEurRate;

    public FixedRateCurrencyConverter(IOptions<CurrencyOptions> options)
    {
        _gbpToEurRate = options.Value.GbpToEurRate;
    }

    public decimal ConvertGbpToEur(decimal pounds)
    {
        return decimal.Round(pounds * _gbpToEurRate, 2, MidpointRounding.AwayFromZero);
    }
}
