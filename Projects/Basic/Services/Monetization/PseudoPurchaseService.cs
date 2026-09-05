using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Services.Monetization;

/// <summary>
/// Provides a pseudo in-app purchase service for testing and development.
/// </summary>
public class PseudoPurchaseService : IInAppPurchaseService
{
    private readonly HashSet<string> _debugPurchased = new();
    private readonly List<IProductInfo> _debugProducts;

    /// <summary>
    /// Gets a value indicating whether purchases are available.
    /// </summary>
    public bool CanMakePayments => true;

    /// <summary>
    /// Occurs when a purchase starts.
    /// </summary>
    public event Action<string>? PurchaseStarted;

    /// <summary>
    /// Occurs when a purchase finishes.
    /// </summary>
    public event Action<string, PurchaseResult>? PurchaseFinished;

    /// <summary>
    /// Initializes a new instance of the <see cref="PseudoPurchaseService"/> class.
    /// </summary>
    /// <param name="debugProducts">
    /// The pseudo products exposed by the service.
    /// </param>
    public PseudoPurchaseService(IProductInfo[] debugProducts)
    {
        ArgumentNullException.ThrowIfNull(debugProducts);

        _debugProducts = debugProducts.ToList();
    }

    /// <summary>
    /// Gets the available pseudo products.
    /// </summary>
    /// <returns>
    /// A list of available pseudo products.
    /// </returns>
    public Task<IReadOnlyList<IProductInfo>> GetAvailableProductsAsync()
    {
        return Task.FromResult<IReadOnlyList<IProductInfo>>(
            _debugProducts);
    }

    /// <summary>
    /// Performs a pseudo purchase for the specified product.
    /// </summary>
    /// <param name="productId">
    /// The identifier of the product to purchase.
    /// </param>
    /// <returns>
    /// A successful purchase result.
    /// </returns>
    public Task<PurchaseResult> PurchaseAsync(string productId)
    {
        ArgumentNullException.ThrowIfNull(productId);

        PurchaseStarted?.Invoke(productId);

        bool alreadyOwned = _debugPurchased.Contains(productId);

        _debugPurchased.Add(productId);

        var result = new PurchaseResult
        {
            Success = true,
            AlreadyOwned = alreadyOwned
        };

        PurchaseFinished?.Invoke(productId, result);

        return Task.FromResult(result);
    }

    /// <summary>
    /// Determines whether the specified product has been purchased.
    /// </summary>
    /// <param name="productId">
    /// The identifier of the product.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the product has been purchased;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsPurchased(string productId)
    {
        ArgumentNullException.ThrowIfNull(productId);

        return _debugPurchased.Contains(productId);
    }

    /// <summary>
    /// Restores pseudo purchases.
    /// </summary>
    /// <remarks>
    /// The pseudo service does not require a restore operation.
    /// </remarks>
    public Task RestorePurchasesAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a pseudo product with a title and description.
    /// </summary>
    /// <param name="id">
    /// The product identifier.
    /// </param>
    /// <param name="title">
    /// The product title.
    /// </param>
    /// <param name="description">
    /// The product description.
    /// </param>
    /// <param name="price">
    /// The product price.
    /// </param>
    /// <returns>
    /// A pseudo product information object.
    /// </returns>
    public static IProductInfo CreatePseudoProduct(
        int id,
        string title,
        string description,
        float price)
    {
        return new DebugProductInfo(
            id,
            title,
            description,
            price);
    }

    /// <summary>
    /// Creates a pseudo product without a description.
    /// </summary>
    /// <param name="id">
    /// The product identifier.
    /// </param>
    /// <param name="title">
    /// The product title.
    /// </param>
    /// <param name="price">
    /// The product price.
    /// </param>
    /// <returns>
    /// A pseudo product information object.
    /// </returns>
    public static IProductInfo CreatePseudoProduct(
        int id,
        string title,
        float price)
    {
        return new DebugProductInfo(
            id,
            title,
            string.Empty,
            price);
    }

    /// <summary>
    /// Provides product information for a pseudo product.
    /// </summary>
    private sealed class DebugProductInfo : IProductInfo
    {
        public string Title { get; }

        public string Description { get; }

        public int Id { get; }

        public float Price { get; }

        public DebugProductInfo(
            int id,
            string title,
            string description,
            float price)
        {
            Id = id;
            Title = title;
            Description = description;
            Price = price;
        }
    }
}