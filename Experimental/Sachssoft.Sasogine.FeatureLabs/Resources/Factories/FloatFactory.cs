using System.Globalization;

namespace Sachssoft.Sasogine.FeatureLabs.Resources.Factories;

internal class FloatFactory : ITypeFactory<float, Resource>
{
    public float Create(ResourceStore store, Resource entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        if (string.IsNullOrWhiteSpace(entry.Content))
            throw new InvalidOperationException(
                $"Resource '{entry.Id ?? "<unnamed>"}' does not contain any content to parse float.");

        if (!float.TryParse(entry.Content, NumberStyles.Float | NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture, out var value))
        {
            throw new InvalidOperationException(
                $"Failed to parse float from resource '{entry.Id ?? "<unnamed>"}': '{entry.Content}'.");
        }

        return value;
    }
}