namespace sachssoft.Sasogine.Services;

public class PurchaseResult
{
    public bool Success { get; set; }
    public bool AlreadyOwned { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Receipt { get; set; } // Optional: zur Server-Verifikation
}