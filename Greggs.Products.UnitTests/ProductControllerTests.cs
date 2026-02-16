using Greggs.Products.Api.Controllers;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Greggs.Products.UnitTests
{
    public class ProductControllerTests
    {
        [Fact]
        public void Get_ReturnsOk_WithProducts_FromDataAccess()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ProductController>>();

            var productsFromDal = new List<Product>
        {
            new() { Name = "Sausage Roll", PriceInPounds = 1.00m },
            new() { Name = "Steak Bake", PriceInPounds = 1.20m }
        };

            var dataAccessMock = new Mock<IDataAccess<Product>>();
            dataAccessMock
                .Setup(d => d.List(0, 5))
                .Returns(productsFromDal);

            var converterMock = new Mock<ICurrencyConverter>();

            var controller = new ProductController(logger, dataAccessMock.Object, converterMock.Object);

            // Act
            var result = controller.Get(pageStart: 0, pageSize: 5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<Product>>(ok.Value);

            Assert.Equal(2, returned.Count());
            Assert.Equal("Sausage Roll", returned.First().Name);

            dataAccessMock.Verify(d => d.List(0, 5), Times.Once);
        }

        [Theory]
        [InlineData(-1, 5, "pageStart must be 0 or greater.")]
        [InlineData(0, 0, "pageSize must be greater than 0.")]
        [InlineData(0, -1, "pageSize must be greater than 0.")]
        public void Get_InvalidPaging_ReturnsBadRequest(int pageStart, int pageSize, string expectedError)
        {
            // Arrange
            var logger = Mock.Of<ILogger<ProductController>>();
            var dataAccessMock = new Mock<IDataAccess<Product>>();
            var converterMock = new Mock<ICurrencyConverter>();

            var controller = new ProductController(logger, dataAccessMock.Object, converterMock.Object);

            // Act
            var result = controller.Get(pageStart, pageSize);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

            // The controller returns: new { error = "..." }
            // We can verify by reading the anonymous object's "error" property via reflection.
            var errorProp = badRequest.Value!.GetType().GetProperty("error");
            Assert.NotNull(errorProp);

            var errorValue = errorProp!.GetValue(badRequest.Value) as string;
            Assert.Equal(expectedError, errorValue);

            dataAccessMock.Verify(d => d.List(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetInEuros_ReturnsOk_WithConvertedPrices()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ProductController>>();

            var productsFromDal = new List<Product>
        {
            new() { Name = "Sausage Roll", PriceInPounds = 1.00m },
            new() { Name = "Yum Yum", PriceInPounds = 0.70m }
        };

            var dataAccessMock = new Mock<IDataAccess<Product>>();
            dataAccessMock
                .Setup(d => d.List(0, 5))
                .Returns(productsFromDal);

            var converterMock = new Mock<ICurrencyConverter>();
            converterMock.Setup(c => c.ConvertGbpToEur(1.00m)).Returns(1.11m);
            converterMock.Setup(c => c.ConvertGbpToEur(0.70m)).Returns(0.78m);

            var controller = new ProductController(logger, dataAccessMock.Object, converterMock.Object);

            // Act
            var result = controller.GetInEuros(pageStart: 0, pageSize: 5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<ProductInEuros>>(ok.Value);

            var list = returned.ToList();
            Assert.Equal(2, list.Count);

            Assert.Equal("Sausage Roll", list[0].Name);
            Assert.Equal(1.11m, list[0].PriceInEuros);

            Assert.Equal("Yum Yum", list[1].Name);
            Assert.Equal(0.78m, list[1].PriceInEuros);

            dataAccessMock.Verify(d => d.List(0, 5), Times.Once);
            converterMock.Verify(c => c.ConvertGbpToEur(1.00m), Times.Once);
            converterMock.Verify(c => c.ConvertGbpToEur(0.70m), Times.Once);
        }

        [Fact]
        public void Get_PageSizeAboveMax_IsLimitedToMax()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ProductController>>();

            var productsFromDal = new List<Product>
        {
            new() { Name = "Sausage Roll", PriceInPounds = 1.00m }
        };

            var dataAccessMock = new Mock<IDataAccess<Product>>();
            dataAccessMock
                .Setup(d => d.List(0, 100)) // MaxPageSize in controller = 100
                .Returns(productsFromDal);

            var converterMock = new Mock<ICurrencyConverter>();

            var controller = new ProductController(logger, dataAccessMock.Object, converterMock.Object);

            // Act
            var result = controller.Get(pageStart: 0, pageSize: 9999);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            dataAccessMock.Verify(d => d.List(0, 100), Times.Once);
        }
    }
}
