using System;
using StoreKit;
using Foundation;

namespace Sachssoft.Sasogine.Platform.iOS;

using System.Collections.Generic;
using System.Threading.Tasks;

public class iOSInAppPurchaseService : NSObject, IInAppPurchaseService, ISKPaymentTransactionObserver
{
    public bool CanMakePayments => SKPaymentQueue.CanMakePayments;

    public event Action<string>? PurchaseStarted;
    public event Action<string, PurchaseResult>? PurchaseFinished;

    private TaskCompletionSource<IReadOnlyList<IProductInfo>>? _productRequestTcs;
    private List<IProductInfo> _products = new();

    public iOSInAppPurchaseService()
    {
        // Registriere dich als Observer
        SKPaymentQueue.DefaultQueue.AddTransactionObserver(this);
    }

    public Task<IReadOnlyList<IProductInfo>> GetAvailableProductsAsync()
    {
        _productRequestTcs = new TaskCompletionSource<IReadOnlyList<IProductInfo>>();

        var productIds = new NSSet("com.sasogine.example1", "com.sasogine.example2");
        var request = new SKProductsRequest(productIds);
        request.ReceivedResponse += (sender, e) =>
        {
            _products.Clear();
            foreach (var p in e.Response.Products)
            {
                _products.Add(new AppleProductInfo(p));
            }
            _productRequestTcs?.TrySetResult(_products.AsReadOnly());
        };
        request.RequestFailed += (sender, e) =>
        {
            _productRequestTcs?.TrySetException(new Exception(e.Error.LocalizedDescription));
        };
        request.Start();

        return _productRequestTcs.Task;
    }

    public async Task<PurchaseResult> PurchaseAsync(string productId)
    {
        var product = _products.Find(p => p.ProductId == productId) as AppleProductInfo;
        if (product?.NativeProduct == null)
            throw new InvalidOperationException("Produkt nicht geladen!");

        PurchaseStarted?.Invoke(productId);

        var payment = SKPayment.CreateFrom(product.NativeProduct);
        SKPaymentQueue.DefaultQueue.AddPayment(payment);

        // Warten auf Rückmeldung → hier eher per Event gesteuert
        var tcs = new TaskCompletionSource<PurchaseResult>();
        // Speichere TCS in Dictionary<productId, TaskCompletionSource<PurchaseResult>> …
        return await tcs.Task;
    }

    public bool IsPurchased(string productId)
    {
        // iOS speichert Non-Consumables über Apple Account
        // Kann man z.B. lokal cachen oder beim Restore prüfen
        return false;
    }

    public Task RestorePurchasesAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();
        // Beende TCS, wenn UpdatedTransactions mit Restored kommt
        return tcs.Task;
    }

    // Wird von iOS aufgerufen, wenn Transaktionen aktualisiert werden
    [Export("paymentQueue:updatedTransactions:")]
    public void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
    {
        foreach (var t in transactions)
        {
            switch (t.TransactionState)
            {
                case SKPaymentTransactionState.Purchased:
                    PurchaseFinished?.Invoke(t.Payment.ProductIdentifier, PurchaseResult.Success);
                    SKPaymentQueue.DefaultQueue.FinishTransaction(t);
                    break;
                case SKPaymentTransactionState.Failed:
                    PurchaseFinished?.Invoke(t.Payment.ProductIdentifier, PurchaseResult.Failed);
                    SKPaymentQueue.DefaultQueue.FinishTransaction(t);
                    break;
                case SKPaymentTransactionState.Restored:
                    PurchaseFinished?.Invoke(t.Payment.ProductIdentifier, PurchaseResult.Restored);
                    SKPaymentQueue.DefaultQueue.FinishTransaction(t);
                    break;
            }
        }
    }
}

public class AppleProductInfo : IProductInfo
{
    public string ProductId => NativeProduct.ProductIdentifier;
    public string Title => NativeProduct.LocalizedTitle;
    public string Description => NativeProduct.LocalizedDescription;
    public string Price => NativeProduct.PriceLocale.CurrencySymbol + NativeProduct.Price;

    internal SKProduct NativeProduct { get; }

    public AppleProductInfo(SKProduct product)
    {
        NativeProduct = product;
    }
}
