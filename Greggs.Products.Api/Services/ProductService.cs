using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.Contracts;
using Greggs.Products.Api.Repositories;

namespace Greggs.Products.Api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICurrencyConverter _currencyConverter;

    public ProductService(IProductRepository productRepository, ICurrencyConverter currencyConverter)
    {
        _productRepository = productRepository;
        _currencyConverter = currencyConverter;
    }

    public async Task<IEnumerable<ProductResponse>> GetProductsAsync(int pageStart, int pageSize, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetProductsAsync(pageStart, pageSize, cancellationToken);

        return products.Select(p => new ProductResponse
        {
            Name = p.Name,
            PriceInPounds = p.PriceInPounds
        });
    }

    public async Task<IEnumerable<ProductEurosResponse>> GetProductsInEurosAsync(int pageStart, int pageSize, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetProductsAsync(pageStart, pageSize, cancellationToken);

        return products.Select(p => new ProductEurosResponse
        {
            Name = p.Name,
            PriceInEuros = _currencyConverter.ConvertGbpToEur(p.PriceInPounds)
        });
    }
}
