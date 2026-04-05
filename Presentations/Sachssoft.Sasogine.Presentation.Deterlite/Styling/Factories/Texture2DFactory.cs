using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    internal class Texture2DFactory : ITypeFactory<Texture2D, Resource>
    {
        public Texture2D Create(Skin skin, Resource entry)
        {
            if (skin == null)
                throw new ArgumentNullException(nameof(skin));
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            try
            {
                using var stream = entry.GetStream(skin);
                var atlas = Texture2D.FromStream(skin.Workspace.Application.GraphicsDevice, stream);

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
}