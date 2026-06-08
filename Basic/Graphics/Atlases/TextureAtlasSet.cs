using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources;

public sealed class TextureAtlasSet
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
}