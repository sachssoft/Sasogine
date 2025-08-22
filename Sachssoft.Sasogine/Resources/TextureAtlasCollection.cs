using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources;

public class TextureAtlasCollection
{
    private readonly List<TextureAtlas> _atlases = new();
    private readonly Dictionary<string, (TextureAtlas atlas, TextureAtlasFrame frame)> _lookup = new();

    public void Add(TextureAtlas atlas)
    {
        if (atlas == null)
            throw new ArgumentNullException(nameof(atlas));

        _atlases.Add(atlas);

        foreach (var kvp in atlas.Frames)
        {
            // letzter gewinnt bei gleichem Namen
            _lookup[kvp.Key] = (atlas, kvp.Value);
        }
    }

    public bool TryGetFrame(string name, out TextureAtlasFrame frame)
    {
        if (_lookup.TryGetValue(name, out var entry))
        {
            frame = entry.frame;
            return true;
        }
        frame = default;
        return false;
    }

    public bool TryDraw(SpriteBatch spriteBatch, string name, Vector2 position, Color color)
    {
        if (_lookup.TryGetValue(name, out var entry))
        {
            spriteBatch.Draw(entry.atlas.Texture, position, entry.frame.SourceRectangle, color);
            return true;
        }
        return false;
    }

    public bool TryCrop(string name, out Texture2D texture)
    {
        texture = null;
        if (_lookup.TryGetValue(name, out var entry))
        {
            texture = entry.atlas.Texture.Crop(entry.frame.SourceRectangle);
            return true;
        }
        return false;
    }
}
