namespace Sachssoft.Engine.Services.Monetization;

/// <summary>
/// Provides store product identity and separately renderable localized price information.
/// </summary>
/// <remarks>
/// Store adapters supply localized strings. Presentation code can render the amount and symbol
/// with different fonts; currency codes identify currencies and are not display symbols.
/// </remarks>
public interface IProductInfo
{
    /// <summary>
    /// Gets the exact store product identifier used by purchase operations.
    /// </summary>
    string Id
    {
        get;
    }

    /// <summary>
    /// Gets the display title of the product.
    /// </summary>
    string Title
    {
        get;
    }

    /// <summary>
    /// Gets the product description, or an empty string.
    /// </summary>
    string Description
    {
        get;
    }

    /// <summary>
    /// Gets the nonnegative numeric price in the product currency.
    /// </summary>
    decimal Price
    {
        get;
    }

    /// <summary>
    /// Gets the uppercase ISO 4217 currency code, such as EUR or USD.
    /// </summary>
    string CurrencyCode
    {
        get;
    }

    /// <summary>
    /// Gets the visible currency symbol, such as €, $, or US$.
    /// </summary>
    string CurrencySymbol
    {
        get;
    }

    /// <summary>
    /// Gets the localized amount without a currency symbol, such as 4,99 or 4.99.
    /// </summary>
    string FormattedAmount
    {
        get;
    }

    /// <summary>
    /// Gets the complete localized price supplied by the store, including its currency symbol.
    /// </summary>
    string LocalizedPrice
    {
        get;
    }

    /// <summary>
    /// Gets whether the currency symbol is displayed before the formatted amount.
    /// </summary>
    bool CurrencySymbolBeforeAmount
    {
        get;
    }
}
