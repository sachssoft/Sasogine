using Sachssoft.Sasogine.Resources;
using System;

namespace Sachssoft.Sasogine.Resources.Factories;

internal class TextureAtlasFactory : ITypeFactory<TextureAtlas, Resource>
{
    public TextureAtlas Create(ResourceStore store, Resource entry)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        try
        {
            using var stream = entry.GetStream(store);
            var atlas = TextureAtlas.Load(stream);

            if (atlas == null)
                throw new InvalidOperationException($"TextureAtlas could not be loaded from resource '{entry.Id}'.");

            return atlas;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create TextureAtlas from resource '{entry.Id}': {ex.Message}", ex);
        }
    }
}