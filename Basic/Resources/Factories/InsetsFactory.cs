using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Resources.Factories;

internal class InsetsFactory : ITypeFactory<Insets, Resource>
{
    public Insets Create(ResourceStore store, Resource entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        if (Insets.TryParse(entry.Content, out var insets))
            return insets;

        throw new InvalidOperationException(
            $"Resource '{entry.Id ?? "<unnamed>"}' has invalid Insets content: '{entry.Content}'");
    }
}