namespace Greggs.Products.Api.Options;

public class CurrencyOptions
{
    public const string SectionName = "Currency";

    public decimal GbpToEurRate { get; set; } = 1.11m;
}
