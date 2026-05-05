namespace Greggs.Products.Api.Contracts;

/// <summary>Represents a product with its GBP price.</summary>
public class ProductResponse
{
    /// <summary>Display name of the product.</summary>
    public string Name { get; set; }

    /// <summary>Price in pounds sterling (GBP).</summary>
    public decimal PriceInPounds { get; set; }
}
