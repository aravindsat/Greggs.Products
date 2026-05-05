using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;

namespace Greggs.Products.Api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDataAccess<Product> _dataAccess;

    public ProductRepository(IDataAccess<Product> dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<IEnumerable<Product>> GetProductsAsync(int pageStart, int pageSize, CancellationToken cancellationToken = default)
    {
        return _dataAccess.ListAsync(pageStart, pageSize, cancellationToken);
    }
}
