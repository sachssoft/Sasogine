using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Services.Monetization;

/// <summary>
/// Provides access to platform-specific in-app purchase services.
/// </summary>
/// <remarks>
/// Product identifiers match <see cref="IProductInfo.Id"/>. Store adapters expose display-ready
/// currency metadata; the application chooses fonts for the amount and currency symbol.
/// </remarks>
public interface IInAppPurchaseService
{
    /// <summary>Occurs when a purchase starts, with its exact store product identifier.</summary>
    event Action<string>? PurchaseStarted;

    /// <summary>Occurs when a purchase finishes, with its identifier and result.</summary>
    event Action<string, PurchaseResult>? PurchaseFinished;

    /// <summary>Gets whether purchases are supported on the current platform.</summary>
    bool CanMakePayments
    {
        get;
    }

    /// <summary>Starts the purchase of an available store product.</summary>
    /// <param name="productId">The exact identifier from <see cref="IProductInfo.Id"/>.</param>
    /// <returns>The result of the purchase operation.</returns>
    Task<PurchaseResult> PurchaseAsync(string productId);

    /// <summary>Determines whether a product is already owned, primarily for non-consumable products.</summary>
    /// <param name="productId">The exact store product identifier.</param>
    /// <returns>Whether the product is already owned.</returns>
    bool IsPurchased(string productId);

    /// <summary>Gets the available store products with localized amount and currency information.</summary>
    /// <returns>The current product catalog.</returns>
    Task<IReadOnlyList<IProductInfo>> GetAvailableProductsAsync();

    /// <summary>Attempts to restore previously purchased products.</summary>
    /// <returns>A task representing the restore operation.</returns>
    Task RestorePurchasesAsync();
}
