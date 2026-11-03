using System;

namespace Sachssoft.Engine.Services.Monetization;

/// <summary>
/// Stores immutable product metadata and localized prices provided by a store adapter.
/// </summary>
public sealed class ProductInfo : IProductInfo
{
    /// <summary>
    /// Initializes a product with explicit store formatting, without parsing a localized price.
    /// </summary>
    /// <param name="id">The exact store product identifier.</param>
    /// <param name="title">The nonempty display title.</param>
    /// <param name="description">The description; an empty string is allowed.</param>
    /// <param name="price">The nonnegative numeric price.</param>
    /// <param name="currencyCode">A three-letter ISO 4217 currency code, normalized to uppercase.</param>
    /// <param name="currencySymbol">The visible currency symbol, which can contain multiple characters.</param>
    /// <param name="formattedAmount">The localized amount without its currency symbol.</param>
    /// <param name="localizedPrice">The complete localized store price.</param>
    /// <param name="currencySymbolBeforeAmount">Whether the symbol precedes the amount.</param>
    /// <exception cref="ArgumentException">A required string is blank or the currency code does not contain three ASCII letters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="description"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="price"/> is negative.</exception>
    public ProductInfo(
        string id,
        string title,
        string description,
        decimal price,
        string currencyCode,
        string currencySymbol,
        string formattedAmount,
        string localizedPrice,
        bool currencySymbolBeforeAmount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencySymbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(formattedAmount);
        ArgumentException.ThrowIfNullOrWhiteSpace(localizedPrice);

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price));
        }

        string normalizedCode = currencyCode.ToUpperInvariant();
        if (normalizedCode.Length != 3 ||
            normalizedCode[0] is < 'A' or > 'Z' ||
            normalizedCode[1] is < 'A' or > 'Z' ||
            normalizedCode[2] is < 'A' or > 'Z')
        {
            throw new ArgumentException("A currency code must contain three ASCII letters.", nameof(currencyCode));
        }

        Id = id;
        Title = title;
        Description = description;
        Price = price;
        CurrencyCode = normalizedCode;
        CurrencySymbol = currencySymbol;
        FormattedAmount = formattedAmount;
        LocalizedPrice = localizedPrice;
        CurrencySymbolBeforeAmount = currencySymbolBeforeAmount;
    }

    /// <inheritdoc/>
    public string Id
    {
        get;
    }

    /// <inheritdoc/>
    public string Title
    {
        get;
    }

    /// <inheritdoc/>
    public string Description
    {
        get;
    }

    /// <inheritdoc/>
    public decimal Price
    {
        get;
    }

    /// <inheritdoc/>
    public string CurrencyCode
    {
        get;
    }

    /// <inheritdoc/>
    public string CurrencySymbol
    {
        get;
    }

    /// <inheritdoc/>
    public string FormattedAmount
    {
        get;
    }

    /// <inheritdoc/>
    public string LocalizedPrice
    {
        get;
    }

    /// <inheritdoc/>
    public bool CurrencySymbolBeforeAmount
    {
        get;
    }
}
