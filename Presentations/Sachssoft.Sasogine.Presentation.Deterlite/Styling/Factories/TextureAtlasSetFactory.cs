using Sachssoft.Sasogine.Resources;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Styling.Factories
{
    internal class TextureAtlasSetFactory : ITypeFactory<TextureAtlasSet, Resource>
    {
        public TextureAtlasSet Create(Skin skin, Resource entry)
        {
            string variantName = "";
            List<TextureAtlas> atlases = new();

            foreach (var property in entry.Properties)
            {
                switch (property.Name)
                {
                    case nameof(TextureAtlasSet.Name):
                        if (property.Value is string name)
                            variantName = name;
                        break;
                }
            }

            // Definierte Texture-Atlanten holen
            foreach (var child in entry.Children)
            {
                if (child.TargetType != typeof(TextureAtlas))
                    continue;

                // File obligatorisch
                if (string.IsNullOrEmpty(child.File))
                    continue;

                if (skin.Registry.TryCreate<TextureAtlas>(child, out var atlas))
                {
                    atlases.Add(atlas);
                }
            }

            return new TextureAtlasSet(variantName, atlases.ToArray());
        }
    }
}
