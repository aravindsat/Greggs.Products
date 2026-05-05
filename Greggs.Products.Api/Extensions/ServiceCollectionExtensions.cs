using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Options;
using Greggs.Products.Api.Repositories;
using Greggs.Products.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Greggs.Products.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        services.AddScoped<IDataAccess<Product>, ProductAccess>();
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }

    public static IServiceCollection AddProductServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    }

    public static IServiceCollection AddCurrencyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CurrencyOptions>(configuration.GetSection(CurrencyOptions.SectionName));
        services.AddSingleton<ICurrencyConverter, FixedRateCurrencyConverter>();
        return services;
    }
}
