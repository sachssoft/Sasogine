namespace Sachssoft.Sasogine.Services.Monetization;

/// <summary>
/// Represents the result of a purchase operation.
/// </summary>
public record class PurchaseResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the purchase was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item was already owned.
    /// </summary>
    public bool AlreadyOwned { get; set; }

    /// <summary>
    /// Gets or sets the error message when the purchase failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the optional purchase receipt used for server verification.
    /// </summary>
    public string? Receipt { get; set; }
}