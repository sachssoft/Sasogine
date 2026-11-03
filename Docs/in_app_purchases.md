# In-app purchases and localized prices

Namespace: `Sachssoft.Engine.Services.Monetization`.

`IInAppPurchaseService` supplies products and purchase operations. `ProductInfo` is an
immutable implementation of `IProductInfo` for adapters and application-owned catalogs.
The engine does not choose fonts or make a real payment through its pseudo service.

## Product prices

| Member | Meaning | EUR example | USD example |
| --- | --- | --- | --- |
| `Id` | Exact string passed to `PurchaseAsync` | `demo.eur` | `demo.usd` |
| `Price` | Numeric `decimal` price | `4.99m` | `4.99m` |
| `CurrencyCode` | Uppercase three-letter code | `EUR` | `USD` |
| `CurrencySymbol` | Visible symbol, including multi-character symbols where appropriate | `€` | `$` |
| `FormattedAmount` | Localized number without the symbol | `4,99` | `4.99` |
| `LocalizedPrice` | Complete localized store price | `4,99 €` | `$4.99` |
| `CurrencySymbolBeforeAmount` | Whether the symbol precedes the number | `false` | `true` |

A real store adapter supplies its authoritative localized price strings directly to
`ProductInfo`; do not infer a currency from the user's language or split a store price
with a regular expression. `CurrencyCode` is not a replacement for `CurrencySymbol`.
Applications may render `FormattedAmount` and `CurrencySymbol` as separate controls with
different fonts. For a single-control price, use `LocalizedPrice`, which also preserves
store-specific spacing and formatting.

## Ready-to-use pseudo demo

The following example is provided for manual development use; it is not executed by
loading the engine. The two cultures require normal .NET culture data.

```csharp
using Sachssoft.Engine.Services.Monetization;

IInAppPurchaseService purchases = PseudoPurchaseService.CreateDemo();
var products = await purchases.GetAvailableProductsAsync();

foreach (IProductInfo product in products)
{
    Console.WriteLine($"{product.Id}: {product.LocalizedPrice}");
    // Bind these independently in the UI:
    // product.FormattedAmount -> application numeric font
    // product.CurrencySymbol -> application text font, bold if desired
    // product.CurrencySymbolBeforeAmount -> order of the controls
}

PurchaseResult first = await purchases.PurchaseAsync("demo.eur");
// Success = true, AlreadyOwned = false

PurchaseResult repeated = await purchases.PurchaseAsync("demo.eur");
// Success = true, AlreadyOwned = true

PurchaseResult unknown = await purchases.PurchaseAsync("not.in.catalog");
// Success = false, ErrorMessage describes the unknown identifier

bool owned = purchases.IsPurchased("demo.eur");
// true

await purchases.RestorePurchasesAsync();
// No-op: this instance keeps its current in-memory ownership.
```

A new `CreateDemo()` instance starts with an empty ownership set. The pseudo service
never contacts a store, charges money, grants game inventory, validates receipts, or
persists ownership. It is not a complete consumable-product or subscription backend.
Start/completion events run synchronously on the calling thread.

## Custom pseudo catalog

```csharp
var products = new[]
{
    PseudoPurchaseService.CreatePseudoProduct(
        id: "preview.gems.small",
        title: "Small gem pack",
        description: "Development catalog entry",
        price: 1.99m,
        currencyCode: "EUR",
        currencySymbol: "€",
        cultureName: "de-DE")
};

var purchases = new PseudoPurchaseService(products);
```

The culture controls decimal places, separators, and symbol order for the preview.
The code and symbol are explicit, allowing currencies such as `USD` with `$` or `US$`.
Duplicate IDs, null entries, and blank IDs are rejected when constructing a catalog.
Negative prices and invalid three-letter currency code syntax are rejected by
`ProductInfo`. Currency code syntax validation does not verify an external ISO registry.

## Migration

This alpha API intentionally changes `IProductInfo.Id` from `int` to `string` and
`IProductInfo.Price` from `float` to `decimal`. Update custom interface implementations
and factory callers. Use stable store IDs, decimal literals such as `4.99m`, and explicit
currency metadata. The former integer-ID/float-price pseudo factory overloads are replaced.
Existing `IInAppPurchaseService` method and event signatures remain unchanged.
