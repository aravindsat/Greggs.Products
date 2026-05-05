using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.Models;

namespace Greggs.Products.Api.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetProductsAsync(int pageStart, int pageSize, CancellationToken cancellationToken = default);
}
