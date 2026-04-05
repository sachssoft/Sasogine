using Sachssoft.Sasogine.Resources;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Styling
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

                atlases.Add(skin.Registry.Create<TextureAtlas>(skin, child));
            }

            return new TextureAtlasSet(variantName, atlases.ToArray());
        }
    }
}
