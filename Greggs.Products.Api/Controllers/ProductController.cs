using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
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

    // Safety limit to prevent someone requesting a massive page size
    private const int MaxPageSize = 100;

    private readonly ILogger<ProductController> _logger;
    private readonly IDataAccess<Product> _productAccess;

    /// <summary>
    /// Initialises a new instance of the <see cref="ProductController"/> class.
    /// </summary>
    /// <param name="logger">Diagnostics logger.</param>
    /// <param name="productAccess">Product data access.</param>
    public ProductController(ILogger<ProductController> logger, IDataAccess<Product> productAccess)
    {
        _logger = logger;
        _productAccess = productAccess;
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
        // Log paging parameters used for the request
        _logger.LogInformation("Products requested. pageStart={PageStart}, pageSize={PageSize}", pageStart, pageSize);

        // Validation
        if (pageStart < 0)
        {
            _logger.LogWarning("Invalid pageStart received: {PageStart}", pageStart);
            return BadRequest(new { error = "pageStart must be 0 or greater." });
        }

        if (pageSize <= 0)
        {
            _logger.LogWarning("Invalid pageSize received: {PageSize}", pageSize);
            return BadRequest(new { error = "pageSize must be greater than 0." });
        }

        // Normalize
        if (pageSize > MaxPageSize)
        {
            _logger.LogWarning("pageSize too large ({PageSize}). Limiting to {MaxPageSize}.", pageSize, MaxPageSize);
            pageSize = MaxPageSize;
        }
        // Retrieve and log products from the data access layer
        var products = _productAccess.List(pageStart, pageSize).ToList();
        _logger.LogInformation("Products returned. count={Count}, pageStart={PageStart}, pageSize={PageSize}", products.Count, pageStart, pageSize);

        return Ok(products);
    }
}