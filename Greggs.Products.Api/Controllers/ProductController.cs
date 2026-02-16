using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Greggs.Products.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private const int DefaultPageStart = 0;
    private const int DefaultPageSize = 5;

    // Safety limit to prevent requesting an excessively large page size
    private const int MaxPageSize = 100;

    private readonly ILogger<ProductController> _logger;
    private readonly IDataAccess<Product> _productAccess;
    private readonly ICurrencyConverter _currencyConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductController"/> class.
    /// </summary>
    /// <param name="logger">Diagnostics logger.</param>
    /// <param name="productAccess">Product data access.</param>
    /// <param name="currencyConverter">Currency conversion service.</param>
    public ProductController(
        ILogger<ProductController> logger,
        IDataAccess<Product> productAccess,
        ICurrencyConverter currencyConverter)
    {
        _logger = logger;
        _productAccess = productAccess;
        _currencyConverter = currencyConverter;
    }

    /// <summary>
    /// Returns the latest menu of products from the data access layer.
    /// </summary>  
    /// <param name="pageStart">Index to begin from.</param>
    /// <param name="pageSize">Maximum number of products to return.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<Product>> Get(int pageStart = DefaultPageStart, int pageSize = DefaultPageSize)
    {
        _logger.LogInformation("GBP products requested. pageStart={PageStart}, pageSize={PageSize}", pageStart, pageSize);

        if (!TryValidateAndNormalizePaging(pageStart, pageSize, out var start, out var size, out var error))
        {
            _logger.LogWarning("Invalid paging. pageStart={PageStart}, pageSize={PageSize}, error={Error}", pageStart, pageSize, error);
            return BadRequest(new { error });
        }

        var products = _productAccess.List(start, size).ToList();

        _logger.LogInformation("GBP products returned. count={Count}, pageStart={PageStart}, pageSize={PageSize}",
            products.Count, start, size);

        return Ok(products);
    }

    /// <summary>
    /// Returns latest menu priced in EUR (via currency conversion service).
    /// </summary>
    /// <param name="pageStart">Index to begin from.</param>
    /// <param name="pageSize">Maximum number of products to return.</param>
    [HttpGet("euros")]
    [ProducesResponseType(typeof(IEnumerable<ProductInEuros>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<ProductInEuros>> GetInEuros(int pageStart = DefaultPageStart, int pageSize = DefaultPageSize)
    {
        _logger.LogInformation("EUR products requested. pageStart={PageStart}, pageSize={PageSize}", pageStart, pageSize);

        if (!TryValidateAndNormalizePaging(pageStart, pageSize, out var start, out var size, out var error))
        {
            _logger.LogWarning("Invalid paging. pageStart={PageStart}, pageSize={PageSize}, error={Error}", pageStart, pageSize, error);
            return BadRequest(new { error });
        }

        var productsInGbp = _productAccess.List(start, size).ToList();

        var productsInEur = productsInGbp
            .Select(p => new ProductInEuros
            {
                Name = p.Name,
                PriceInEuros = _currencyConverter.ConvertGbpToEur(p.PriceInPounds)
            })
            .ToList();

        _logger.LogInformation("EUR products returned. count={Count}, pageStart={PageStart}, pageSize={PageSize}",
            productsInEur.Count, start, size);

        return Ok(productsInEur);
    }

    private static bool TryValidateAndNormalizePaging( int pageStart, int pageSize, out int normalizedStart, out int normalizedSize, out string error)
    {
        normalizedStart = DefaultPageStart;
        normalizedSize = DefaultPageSize;
        error = string.Empty;

        // Validation
        if (pageStart < 0)
        {
            error = "pageStart must be 0 or greater.";
            return false;
        }

        if (pageSize <= 0)
        {
            error = "pageSize must be greater than 0.";
            return false;
        }

        // Normalization
        normalizedStart = pageStart;
        normalizedSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        return true;
    }
}