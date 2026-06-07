using System;

namespace Sachssoft.Sasogine.Resources.Factories;

internal class IntegerFactory : ITypeFactory<int, Resource>
{
    public int Create(ResourceStore store, Resource entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        if (string.IsNullOrWhiteSpace(entry.Content))
            throw new InvalidOperationException(
                $"Resource '{entry.Id ?? "<unnamed>"}' does not contain any content to parse int.");

        if (!int.TryParse(entry.Content, System.Globalization.NumberStyles.Integer,
                          System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            throw new InvalidOperationException(
                $"Failed to parse int from resource '{entry.Id ?? "<unnamed>"}': '{entry.Content}'.");
        }

        return value;
    }
}