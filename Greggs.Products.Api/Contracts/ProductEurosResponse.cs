namespace Greggs.Products.Api.Contracts;

/// <summary>Represents a product with its EUR price.</summary>
public class ProductEurosResponse
{
    /// <summary>Display name of the product.</summary>
    public string Name { get; set; }

    /// <summary>Price in euros (EUR).</summary>
    public decimal PriceInEuros { get; set; }
}
