using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.Contracts;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Greggs.Products.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class ProductController : ControllerBase
{
    private const int DefaultPageStart = 0;
    private const int DefaultPageSize = 5;
    private const int MaxPageSize = 100;

    private readonly ILogger<ProductController> _logger;
    private readonly IProductService _productService;

    public ProductController(ILogger<ProductController> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    /// <summary>Returns the latest menu of products priced in GBP.</summary>
    /// <param name="pageStart">Zero-based index to begin from.</param>
    /// <param name="pageSize">Maximum number of products to return (capped at 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> Get(
        int pageStart = DefaultPageStart,
        int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        if (!TryValidateAndNormalizePaging(pageStart, pageSize, out var start, out var size, out var error))
        {
            _logger.LogWarning("Invalid paging. pageStart={PageStart}, pageSize={PageSize}, error={Error}", pageStart, pageSize, error);
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = error });
        }

        _logger.LogInformation("GBP products requested. pageStart={PageStart}, pageSize={PageSize}", start, size);

        var products = await _productService.GetProductsAsync(start, size, cancellationToken);

        return Ok(products);
    }

    /// <summary>Returns the latest menu of products priced in EUR.</summary>
    /// <param name="pageStart">Zero-based index to begin from.</param>
    /// <param name="pageSize">Maximum number of products to return (capped at 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("euros")]
    [ProducesResponseType(typeof(IEnumerable<ProductEurosResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductEurosResponse>>> GetInEuros(
        int pageStart = DefaultPageStart,
        int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        if (!TryValidateAndNormalizePaging(pageStart, pageSize, out var start, out var size, out var error))
        {
            _logger.LogWarning("Invalid paging. pageStart={PageStart}, pageSize={PageSize}, error={Error}", pageStart, pageSize, error);
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = error });
        }

        _logger.LogInformation("EUR products requested. pageStart={PageStart}, pageSize={PageSize}", start, size);

        var products = await _productService.GetProductsInEurosAsync(start, size, cancellationToken);

        return Ok(products);
    }

    private static bool TryValidateAndNormalizePaging(int pageStart, int pageSize, out int normalizedStart, out int normalizedSize, out string error)
    {
        normalizedStart = DefaultPageStart;
        normalizedSize = DefaultPageSize;
        error = string.Empty;

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

        normalizedStart = pageStart;
        normalizedSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        return true;
    }
}
