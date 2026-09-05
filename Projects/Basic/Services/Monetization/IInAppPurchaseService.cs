using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Services.Monetization
{
    /// <summary>
    /// Provides access to platform-specific in-app purchase services.
    /// </summary>
    public interface IInAppPurchaseService
    {
        /// <summary>
        /// Starts the purchase of a product.
        /// </summary>
        /// <param name="productId">
        /// The identifier of the product to purchase.
        /// </param>
        /// <returns>
        /// A task containing the result of the purchase operation.
        /// </returns>
        Task<PurchaseResult> PurchaseAsync(string productId);

        /// <summary>
        /// Determines whether a product has already been purchased.
        /// This is primarily used for non-consumable products.
        /// </summary>
        /// <param name="productId">
        /// The identifier of the product.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the product has been purchased;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool IsPurchased(string productId);

        /// <summary>
        /// Gets all products currently available from the store.
        /// </summary>
        /// <returns>
        /// A task containing the available products.
        /// </returns>
        Task<IReadOnlyList<IProductInfo>> GetAvailableProductsAsync();

        /// <summary>
        /// Gets a value indicating whether purchases are supported
        /// on the current platform.
        /// </summary>
        bool CanMakePayments { get; }

        /// <summary>
        /// Attempts to restore previously purchased products.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous restore operation.
        /// </returns>
        Task RestorePurchasesAsync();

        /// <summary>
        /// Occurs when a purchase is started.
        /// </summary>
        event Action<string>? PurchaseStarted;

        /// <summary>
        /// Occurs when a purchase is completed.
        /// </summary>
        event Action<string, PurchaseResult>? PurchaseFinished;
    }
}