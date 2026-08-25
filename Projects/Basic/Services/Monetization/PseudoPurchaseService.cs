using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Services.Monetization;

public class PseudoPurchaseService : IInAppPurchaseService
{
    private readonly HashSet<string> _debugPurchased = new();
    private readonly List<IProductInfo> _debugProducts;

    public bool CanMakePayments => true;

    public event Action<string>? PurchaseStarted;
    public event Action<string, PurchaseResult>? PurchaseFinished;

    public PseudoPurchaseService(IProductInfo[] debugProducts)
    {
        // Einfach zwei Fake-Produkte definieren
        _debugProducts = debugProducts.ToList();
    }

    event Action<string, PurchaseResult>? IInAppPurchaseService.PurchaseFinished
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    public Task<IReadOnlyList<IProductInfo>> GetAvailableProductsAsync()
    {
        // Gibt sofort die Fake-Produkte zurück
        return Task.FromResult<IReadOnlyList<IProductInfo>>(_debugProducts);
    }

    public Task<PurchaseResult> PurchaseAsync(string productId)
    {
        PurchaseStarted?.Invoke(productId);

        // Debug: Immer erfolgreich
        _debugPurchased.Add(productId);

        var result = new PurchaseResult()
        {
            Success = true,
            AlreadyOwned = true
        };
        //var result = new PurchaseResult(true, $"[DEBUG] {productId} als gekauft markiert");
        PurchaseFinished?.Invoke(productId, result);

        return Task.FromResult(result);
    }

    public bool IsPurchased(string productId)
    {
        // Debug: Nur was gekauft wurde, ist „aktiv“
        return _debugPurchased.Contains(productId);
    }

    public Task RestorePurchasesAsync()
    {
        // Debug: Nichts zu tun
        return Task.CompletedTask;
    }

    public static IProductInfo CreatePseudoProduct(int id, string title, string description, float price)
    {
        return new DebugProductInfo(id, title, description, price);
    }

    public static IProductInfo CreatePseudoProduct(int id, string title, float price)
    {
        return new DebugProductInfo(id, title, string.Empty, price);
    }

    // Einfache Fake-Produktinfo
    private class DebugProductInfo : IProductInfo
    {
        public string Title { get; }
        public string Description { get; }
        public int Id { get; }
        public float Price { get; }

        public DebugProductInfo(int id, string title, string description, float price)
        {
            Id = id;
            Title = title;
            Description = description;
            Price = price;
        }
    }
}
