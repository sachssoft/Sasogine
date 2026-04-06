using System;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Resources.Factories;

internal class BoundsFactory : ITypeFactory<Bounds, Resource>
{
    public Bounds Create(ResourceStore store, Resource entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        if (string.IsNullOrWhiteSpace(entry.Content))
            throw new InvalidOperationException(
                $"Resource '{entry.Id ?? "<unnamed>"}' does not contain any content to parse Bounds.");

        try
        {
            // Parse Bounds aus Content (z. B. "x,y,width,height")
            return Bounds.Parse(entry.Content);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse Bounds from resource '{entry.Id ?? "<unnamed>"}'.",
                ex);
        }
    }
}