namespace Sachssoft.Sasogine.FeatureLabs.Resources.Factories;

internal class TextureAtlasSetFactory : ITypeFactory<TextureAtlasSet, Resource>
{
    public TextureAtlasSet Create(ResourceStore store, Resource entry)
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
            if (child is Resource resChild && resChild.TargetType == typeof(TextureAtlas))
            {
                // File obligatorisch
                if (string.IsNullOrEmpty(resChild.File))
                    continue;

                atlases.Add(store.Registry.Create<TextureAtlas>(store, child));
            }
        }

        return new TextureAtlasSet(variantName, atlases.ToArray());
    }
}
