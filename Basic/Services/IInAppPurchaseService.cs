using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Services;

public interface IInAppPurchaseService
{
    /// <summary>
    /// Startet den Kauf eines Produkts.
    /// </summary>
    /// <param name="productId">ID des zu kaufenden Produkts.</param>
    Task<PurchaseResult> PurchaseAsync(string productId);

    /// <summary>
    /// Prüft, ob ein Produkt bereits gekauft wurde (nur für non-consumables).
    /// </summary>
    bool IsPurchased(string productId);

    /// <summary>
    /// Gibt eine Liste aller im Store verfügbaren Produkte zurück.
    /// </summary>
    Task<IReadOnlyList<IProductInfo>> GetAvailableProductsAsync();

    /// <summary>
    /// Gibt an, ob Käufe auf dieser Plattform grundsätzlich möglich sind.
    /// </summary>
    bool CanMakePayments { get; }

    /// <summary>
    /// (Optional) Versucht, bereits gekaufte Produkte wiederherzustellen (z. B. nach Gerätewechsel).
    /// </summary>
    Task RestorePurchasesAsync();

    /// <summary>
    /// (Optional) Wird ausgelöst, wenn ein Kauf gestartet wurde.
    /// </summary>
    event Action<string>? PurchaseStarted;

    /// <summary>
    /// (Optional) Wird ausgelöst, wenn ein Kauf abgeschlossen wurde.
    /// </summary>
    event Action<string, PurchaseResult>? PurchaseFinished;
}
