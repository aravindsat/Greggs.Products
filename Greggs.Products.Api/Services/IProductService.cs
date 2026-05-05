using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.Contracts;

namespace Greggs.Products.Api.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetProductsAsync(int pageStart, int pageSize, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductEurosResponse>> GetProductsInEurosAsync(int pageStart, int pageSize, CancellationToken cancellationToken = default);
}
