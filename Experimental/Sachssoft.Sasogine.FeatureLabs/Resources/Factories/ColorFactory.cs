namespace Sachssoft.Sasogine.FeatureLabs.Resources.Factories;

internal class ColorFactory : ITypeFactory<Color, Resource>
{
    public Color Create(ResourceStore store, Resource entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        var content = entry.Content?.Trim();

        if (string.IsNullOrEmpty(content))
        {
            throw new InvalidOperationException($"Resource '{entry.Id ?? "<unnamed>"}' has no content for Color.");
        }

        // Robustes Parsen, wirft nur bei ungültigem Format
        try
        {
            return ColorUtils.FromHex(content);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                $"Resource '{entry.Id ?? "<unnamed>"}' has invalid hex content for Color: '{content}'", ex);
        }
    }
}