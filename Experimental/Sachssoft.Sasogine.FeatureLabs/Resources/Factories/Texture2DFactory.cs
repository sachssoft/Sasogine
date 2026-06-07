using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Resources.Factories;

internal class Texture2DFactory : ITypeFactory<Texture2D, Resource>
{
    public Texture2D Create(ResourceStore store, Resource entry)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        try
        {
            using var stream = entry.GetStream(store);
            var atlas = Texture2D.FromStream(store.Application.GraphicsDevice, stream);

            if (atlas == null)
                throw new InvalidOperationException($"Texture2D could not be loaded from resource '{entry.Id}'.");

            return atlas;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Texture2D from resource '{entry.Id}': {ex.Message}", ex);
        }
    }
}