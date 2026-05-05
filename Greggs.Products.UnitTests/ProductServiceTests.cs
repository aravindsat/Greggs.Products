using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Repositories;
using Greggs.Products.Api.Services;
using Moq;
using Xunit;

namespace Greggs.Products.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<ICurrencyConverter> _converterMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _converterMock = new Mock<ICurrencyConverter>();
        _service = new ProductService(_repositoryMock.Object, _converterMock.Object);
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsMappedProductResponses()
    {
        var products = new List<Product>
        {
            new() { Name = "Sausage Roll", PriceInPounds = 1.00m },
            new() { Name = "Steak Bake", PriceInPounds = 1.20m }
        };
        _repositoryMock.Setup(r => r.GetProductsAsync(0, 5, default)).ReturnsAsync(products);

        var result = (await _service.GetProductsAsync(0, 5)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Sausage Roll", result[0].Name);
        Assert.Equal(1.00m, result[0].PriceInPounds);
        Assert.Equal("Steak Bake", result[1].Name);
        Assert.Equal(1.20m, result[1].PriceInPounds);
    }

    [Fact]
    public async Task GetProductsAsync_EmptyRepository_ReturnsEmptyCollection()
    {
        _repositoryMock.Setup(r => r.GetProductsAsync(0, 5, default)).ReturnsAsync(Enumerable.Empty<Product>());

        var result = await _service.GetProductsAsync(0, 5);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProductsInEurosAsync_ConvertsPriceForEachProduct()
    {
        var products = new List<Product>
        {
            new() { Name = "Sausage Roll", PriceInPounds = 1.00m },
            new() { Name = "Yum Yum", PriceInPounds = 0.70m }
        };
        _repositoryMock.Setup(r => r.GetProductsAsync(0, 5, default)).ReturnsAsync(products);
        _converterMock.Setup(c => c.ConvertGbpToEur(1.00m)).Returns(1.11m);
        _converterMock.Setup(c => c.ConvertGbpToEur(0.70m)).Returns(0.78m);

        var result = (await _service.GetProductsInEurosAsync(0, 5)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Sausage Roll", result[0].Name);
        Assert.Equal(1.11m, result[0].PriceInEuros);
        Assert.Equal("Yum Yum", result[1].Name);
        Assert.Equal(0.78m, result[1].PriceInEuros);
        _converterMock.Verify(c => c.ConvertGbpToEur(1.00m), Times.Once);
        _converterMock.Verify(c => c.ConvertGbpToEur(0.70m), Times.Once);
    }

    [Fact]
    public async Task GetProductsInEurosAsync_EmptyRepository_ReturnsEmptyCollectionWithoutCallingConverter()
    {
        _repositoryMock.Setup(r => r.GetProductsAsync(0, 5, default)).ReturnsAsync(Enumerable.Empty<Product>());

        var result = await _service.GetProductsInEurosAsync(0, 5);

        Assert.Empty(result);
        _converterMock.Verify(c => c.ConvertGbpToEur(It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task GetProductsAsync_DelegatesToRepositoryWithCorrectPagingParameters()
    {
        _repositoryMock.Setup(r => r.GetProductsAsync(10, 20, default)).ReturnsAsync(Enumerable.Empty<Product>());

        await _service.GetProductsAsync(10, 20);

        _repositoryMock.Verify(r => r.GetProductsAsync(10, 20, default), Times.Once);
    }
}
