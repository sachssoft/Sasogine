using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Resources;

public sealed class TextureAtlasSet : IResourceContainer
{
    private readonly HashSet<TextureAtlas> _atlases = new();
    private string variantName;
    private TextureAtlas[] textureAtlas;

    public TextureAtlasSet(string variantName, TextureAtlas[] textureAtlas)
    {
        this.variantName = variantName;
        this.textureAtlas = textureAtlas;
    }

    public string Name { get; }

    public bool TryGetResource<T>(string id, out T value)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetResource<T>(string id, [MaybeNullWhen(false)] out T value, ResourceLookupOptions options = ResourceLookupOptions.None)
    {
        throw new System.NotImplementedException();
    }
}