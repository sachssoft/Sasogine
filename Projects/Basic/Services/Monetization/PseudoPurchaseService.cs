using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Services.Monetization;

/// <summary>
/// Provides a catalog-backed, in-memory purchase simulation for development without real payments.
/// </summary>
/// <remarks>
/// Ownership is not persisted and purchases are not verified by a store or server.
/// Repeated purchases report ownership; callers decide whether their product is consumable.
/// </remarks>
public class PseudoPurchaseService : IInAppPurchaseService
{
    /// <summary>
    /// Looks up registered products by their exact store identifiers.
    /// </summary>
    private readonly Dictionary<string, IProductInfo> _debugProductsById = new(StringComparer.Ordinal);

    /// <summary>
    /// Stores purchased identifiers; access is synchronized on this set.
    /// </summary>
    private readonly HashSet<string> _debugPurchased = new(StringComparer.Ordinal);

    /// <summary>
    /// Retains a read-only snapshot of the configured catalog.
    /// </summary>
    private readonly IReadOnlyList<IProductInfo> _debugProducts;

    /// <inheritdoc/>
    public event Action<string>? PurchaseStarted;

    /// <inheritdoc/>
    public event Action<string, PurchaseResult>? PurchaseFinished;

    /// <summary>
    /// Initializes the simulation with a snapshot of available products.
    /// </summary>
    /// <param name="debugProducts">Products with nonempty, unique identifiers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="debugProducts"/> is null.</exception>
    /// <exception cref="ArgumentException">A product is null, has a blank identifier, or repeats an identifier.</exception>
    public PseudoPurchaseService(IProductInfo[] debugProducts)
    {
        ArgumentNullException.ThrowIfNull(debugProducts);
        var snapshot = (IProductInfo[])debugProducts.Clone();

        foreach (IProductInfo product in snapshot)
        {
            if (product is null || string.IsNullOrWhiteSpace(product.Id))
            {
                throw new ArgumentException("Products must have nonempty identifiers.", nameof(debugProducts));
            }

            if (!_debugProductsById.TryAdd(product.Id, product))
            {
                throw new ArgumentException("Product identifiers must be unique.", nameof(debugProducts));
            }
        }

        _debugProducts = Array.AsReadOnly(snapshot);
    }

    /// <inheritdoc/>
    public bool CanMakePayments => true;

    /// <inheritdoc/>
    public Task<IReadOnlyList<IProductInfo>> GetAvailableProductsAsync()
    {
        return Task.FromResult(_debugProducts);
    }

    /// <summary>
    /// Simulates a registered product purchase and raises start and completion events.
    /// </summary>
    /// <param name="productId">The exact identifier from <see cref="IProductInfo.Id"/>.</param>
    /// <returns>A successful simulation result, or a failure for an unknown product.</returns>
    /// <exception cref="ArgumentException"><paramref name="productId"/> is null, empty, or whitespace.</exception>
    public Task<PurchaseResult> PurchaseAsync(string productId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        PurchaseStarted?.Invoke(productId);

        PurchaseResult result;
        if (!_debugProductsById.ContainsKey(productId))
        {
            result = new PurchaseResult
            {
                Success = false,
                ErrorMessage = "Unknown product: " + productId
            };
        }
        else
        {
            bool alreadyOwned;
            lock (_debugPurchased)
            {
                alreadyOwned = !_debugPurchased.Add(productId);
            }

            result = new PurchaseResult
            {
                Success = true,
                AlreadyOwned = alreadyOwned
            };
        }

        PurchaseFinished?.Invoke(productId, result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Checks ownership recorded in this simulation instance.
    /// </summary>
    /// <param name="productId">The exact store product identifier.</param>
    /// <returns>Whether the identifier was purchased successfully in this instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="productId"/> is null, empty, or whitespace.</exception>
    public bool IsPurchased(string productId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        lock (_debugPurchased)
        {
            return _debugPurchased.Contains(productId);
        }
    }

    /// <summary>
    /// Completes without changing the current in-memory ownership set.
    /// </summary>
    /// <returns>An already completed task.</returns>
    /// <remarks>A new instance has no purchases to restore; no persistent store is contacted.</remarks>
    public Task RestorePurchasesAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a product with explicit currency metadata and culture-based preview formatting.
    /// </summary>
    /// <param name="id">The exact store product identifier.</param>
    /// <param name="title">The display title.</param>
    /// <param name="description">The product description.</param>
    /// <param name="price">The nonnegative decimal price.</param>
    /// <param name="currencyCode">The three-letter currency code, such as EUR.</param>
    /// <param name="currencySymbol">The visible symbol, such as €.</param>
    /// <param name="cultureName">The preview formatting culture, such as de-DE.</param>
    /// <returns>An immutable product with separate amount, symbol, and complete price text.</returns>
    /// <exception cref="ArgumentException">Required text or currency metadata is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="price"/> is negative.</exception>
    /// <exception cref="CultureNotFoundException"><paramref name="cultureName"/> is not available.</exception>
    public static IProductInfo CreatePseudoProduct(
        string id,
        string title,
        string description,
        decimal price,
        string currencyCode,
        string currencySymbol,
        string cultureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencySymbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureName);

        var format = (NumberFormatInfo)CultureInfo.GetCultureInfo(cultureName).NumberFormat.Clone();
        format.CurrencySymbol = currencySymbol;
        format.NumberDecimalDigits = format.CurrencyDecimalDigits;
        format.NumberDecimalSeparator = format.CurrencyDecimalSeparator;
        format.NumberGroupSeparator = format.CurrencyGroupSeparator;
        format.NumberGroupSizes = format.CurrencyGroupSizes;

        return new ProductInfo(
            id,
            title,
            description,
            price,
            currencyCode,
            currencySymbol,
            price.ToString("N", format),
            price.ToString("C", format),
            format.CurrencyPositivePattern is 0 or 2);
    }

    /// <summary>
    /// Creates a localized preview product without a description.
    /// </summary>
    /// <param name="id">The exact store product identifier.</param>
    /// <param name="title">The display title.</param>
    /// <param name="price">The nonnegative decimal price.</param>
    /// <param name="currencyCode">The three-letter currency code.</param>
    /// <param name="currencySymbol">The visible currency symbol.</param>
    /// <param name="cultureName">The preview formatting culture.</param>
    /// <returns>An immutable localized preview product.</returns>
    /// <exception cref="ArgumentException">Required text or currency metadata is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="price"/> is negative.</exception>
    /// <exception cref="CultureNotFoundException"><paramref name="cultureName"/> is not available.</exception>
    public static IProductInfo CreatePseudoProduct(
        string id,
        string title,
        decimal price,
        string currencyCode,
        string currencySymbol,
        string cultureName)
    {
        return CreatePseudoProduct(id, title, string.Empty, price, currencyCode, currencySymbol, cultureName);
    }

    /// <summary>
    /// Creates a ready-to-use simulation with German euro and US dollar sample products.
    /// </summary>
    /// <returns>A new isolated simulation exposing demo.eur and demo.usd at 4.99 units each.</returns>
    /// <exception cref="CultureNotFoundException">The de-DE or en-US culture is not available.</exception>
    public static PseudoPurchaseService CreateDemo()
    {
        return new PseudoPurchaseService(new[]
        {
            CreatePseudoProduct("demo.eur", "Euro demo", "German price formatting", 4.99m, "EUR", "€", "de-DE"),
            CreatePseudoProduct("demo.usd", "Dollar demo", "US price formatting", 4.99m, "USD", "$", "en-US")
        });
    }
}
