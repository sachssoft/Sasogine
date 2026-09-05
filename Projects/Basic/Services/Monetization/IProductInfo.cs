namespace Sachssoft.Sasogine.Services.Monetization;

/// <summary>
/// Provides information about a product available for purchase.
/// </summary>
public interface IProductInfo
{
    /// <summary>
    /// Gets the product identifier.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets the display title of the product.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the product description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the product price.
    /// </summary>
    float Price { get; }
}