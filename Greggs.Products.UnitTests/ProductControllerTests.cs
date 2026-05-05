using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.Contracts;
using Greggs.Products.Api.Controllers;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Greggs.Products.UnitTests;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        _controller = new ProductController(Mock.Of<ILogger<ProductController>>(), _serviceMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsOk_WithProductsFromService()
    {
        var products = new List<ProductResponse>
        {
            new() { Name = "Sausage Roll", PriceInPounds = 1.00m },
            new() { Name = "Steak Bake", PriceInPounds = 1.20m }
        };
        _serviceMock.Setup(s => s.GetProductsAsync(0, 5, default)).ReturnsAsync(products);

        var result = await _controller.Get(pageStart: 0, pageSize: 5);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(ok.Value);
        Assert.Equal(2, returned.Count());
        Assert.Equal("Sausage Roll", returned.First().Name);
        _serviceMock.Verify(s => s.GetProductsAsync(0, 5, default), Times.Once);
    }

    [Fact]
    public async Task Get_EmptyProductList_ReturnsOkWithEmptyCollection()
    {
        _serviceMock.Setup(s => s.GetProductsAsync(0, 5, default)).ReturnsAsync(Enumerable.Empty<ProductResponse>());

        var result = await _controller.Get(pageStart: 0, pageSize: 5);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(ok.Value);
        Assert.Empty(returned);
    }

    [Theory]
    [InlineData(-1, 5, "pageStart must be 0 or greater.")]
    [InlineData(0, 0, "pageSize must be greater than 0.")]
    [InlineData(0, -1, "pageSize must be greater than 0.")]
    public async Task Get_InvalidPaging_ReturnsBadRequestWithProblemDetails(int pageStart, int pageSize, string expectedError)
    {
        var result = await _controller.Get(pageStart, pageSize);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(expectedError, problem.Title);
        _serviceMock.Verify(s => s.GetProductsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_PageSizeAboveMax_IsLimitedTo100()
    {
        _serviceMock.Setup(s => s.GetProductsAsync(0, 100, default)).ReturnsAsync(new List<ProductResponse>());

        await _controller.Get(pageStart: 0, pageSize: 9999);

        _serviceMock.Verify(s => s.GetProductsAsync(0, 100, default), Times.Once);
    }

    [Fact]
    public async Task GetInEuros_ReturnsOk_WithEuroPricedProductsFromService()
    {
        var products = new List<ProductEurosResponse>
        {
            new() { Name = "Sausage Roll", PriceInEuros = 1.11m },
            new() { Name = "Yum Yum", PriceInEuros = 0.78m }
        };
        _serviceMock.Setup(s => s.GetProductsInEurosAsync(0, 5, default)).ReturnsAsync(products);

        var result = await _controller.GetInEuros(pageStart: 0, pageSize: 5);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<ProductEurosResponse>>(ok.Value).ToList();
        Assert.Equal(2, returned.Count);
        Assert.Equal(1.11m, returned[0].PriceInEuros);
        Assert.Equal(0.78m, returned[1].PriceInEuros);
    }

    [Theory]
    [InlineData(-1, 5, "pageStart must be 0 or greater.")]
    [InlineData(0, 0, "pageSize must be greater than 0.")]
    public async Task GetInEuros_InvalidPaging_ReturnsBadRequestWithProblemDetails(int pageStart, int pageSize, string expectedError)
    {
        var result = await _controller.GetInEuros(pageStart, pageSize);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(expectedError, problem.Title);
        _serviceMock.Verify(s => s.GetProductsInEurosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
