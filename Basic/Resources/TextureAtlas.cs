using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;

namespace Sachssoft.Sasogine.Resources;

public class TextureAtlas : IResourceContainer
{
    private Texture2D _texture;
    private int _scaleDefinition = 1;
    private readonly Dictionary<string, TextureAtlasFrame> _frames = new();

    public TextureAtlas() { }

    public Texture2D Texture => _texture;

    public int ScaleDefinition => _scaleDefinition;

    public IReadOnlyDictionary<string, TextureAtlasFrame> Frames => _frames;

    public TextureAtlasFrame this[string key] => _frames[key];

    public static TextureAtlas? Load(LoaderBase? loader)
    {
        throw new NotImplementedException();
        //throw new NotImplementedException();
    }

    public static TextureAtlas? Load(Stream stream)
    {
        throw new NotImplementedException();
        //throw new NotImplementedException();
    }

    public void Load(Texture2D texture, Stream xmlStream, int scaleDefinition = 1)
    {
        _texture = texture;

        var doc = XDocument.Load(xmlStream);

        // Unterstützte Element-Namen (alle Case-Insensitive):
        var elementNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sprite",       // Free Texture Packer / Shoebox
            "frame",        // Spine XML (nicht JSON!)
            "subtexture",   // LibGDX / Starling / Sparrow
            "image",        // Shoebox Legacy
            "atlassprite",  // Manche Unity-Tools
            "element"       // (selten) Tiled Export
        };

        foreach (var el in doc.Root.Elements())
        {
            // Element ignorieren, wenn es kein unterstütztes ist
            if (!elementNames.Contains(el.Name.LocalName))
                continue;

            // Name-Attribute:
            // "n"     → Free Texture Packer
            // "name"  → Starling / Sparrow / Shoebox
            string name = el.Attribute("n")?.Value
                       ?? el.Attribute("name")?.Value
                       ?? "";

            // Positions-/Größen-Attribute (verschiedene Schreibweisen):
            // x/y/w/h          → Free Texture Packer
            // x/y/width/height → Starling / LibGDX
            // left/top/width/height → Shoebox
            int x = ParseInt(el, "x", "left");
            int y = ParseInt(el, "y", "top");
            int w = ParseInt(el, "w", "width");
            int h = ParseInt(el, "h", "height");

            // Optional: Rotation-Info (Starling/Sparrow/LibGDX)
            bool rotated = bool.TryParse(el.Attribute("rotated")?.Value, out var r) && r;

            // Optional: Frame-Offsets (Starling/Sparrow)
            int frameX = ParseInt(el, "frameX");
            int frameY = ParseInt(el, "frameY");
            int frameW = ParseInt(el, "frameWidth");
            int frameH = ParseInt(el, "frameHeight");

            if (!string.IsNullOrEmpty(name))
            {
                // Frame-Objekt anlegen (ggf. Rotation/Offsets beachten)
                var frame = new TextureAtlasFrame(
                    name,
                    new Rectangle(x, y, w, h),
                    rotated,
                    new Rectangle(frameX, frameY, frameW, frameH)
                );

                _frames[name] = frame;
            }
        }
    }

    // Hilfsfunktion: sucht mehrere mögliche Attributnamen
    private int ParseInt(XElement el, params string[] keys)
    {
        foreach (var key in keys)
        {
            var attr = el.Attribute(key);
            if (attr != null && int.TryParse(attr.Value, out int result))
                return result;
        }
        return 0;
    }

    public void Draw(SpriteBatch spriteBatch, string spriteName, Vector2 position, Color color)
    {
        if (!_frames.TryGetValue(spriteName, out var frame))
            return;

        spriteBatch.Draw(_texture, position, frame.SourceRectangle, color);
    }

    public Texture2D Crop(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Sprite name cannot be null or empty.", nameof(name));

        if (_texture == null)
            throw new InvalidOperationException("Texture is not loaded.");

        if (!_frames.TryGetValue(name, out var frame))
            throw new KeyNotFoundException($"Frame '{name}' not found in the atlas.");

        return _texture.Crop(frame.SourceRectangle);
    }

    public bool TryCrop(string name, [MaybeNullWhen(false)] out Texture2D croppedTexture)
    {
        croppedTexture = null;

        if (string.IsNullOrEmpty(name) || _texture == null)
            return false;

        if (!_frames.TryGetValue(name, out var frame))
            return false;

        croppedTexture = _texture.Crop(frame.SourceRectangle);
        return true;
    }

    public bool TryGetResource<T>(string id, out T value)
    {
        throw new NotImplementedException();
    }

    public bool TryGetResource<T>(string id, [MaybeNullWhen(false)] out T value, ResourceLookupOptions options = ResourceLookupOptions.None)
    {
        throw new NotImplementedException();
    }
}