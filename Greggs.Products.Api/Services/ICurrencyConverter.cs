namespace Greggs.Products.Api.Services;

public interface ICurrencyConverter
{
    decimal ConvertGbpToEur(decimal pounds);
}
