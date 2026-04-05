using Sachssoft.Sasogine.Resources;
using System;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    internal class TextureAtlasFactory : ITypeFactory<TextureAtlas, Resource>
    {
        public TextureAtlas Create(Skin skin, Resource entry)
        {
            if (skin == null)
                throw new ArgumentNullException(nameof(skin));
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            try
            {
                using var stream = entry.GetStream(skin);
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
}