using FontStashSharp;
using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Linq;

internal class FontStashSharpBackend : IFontBackend
{
    private readonly Dictionary<string, List<FontFaceEntry>> _fonts = new();

    // map (Face + size) -> SpriteFontBase
    private readonly Dictionary<(FontFace Face, float Size), SpriteFontBase> _cache = new();

    private record FontFaceEntry(FontFace Face, FontSystem System);

    public void Register(FontFamily fontFamily)
    {
        foreach (var Face in fontFamily.Faces)
            Register(fontFamily.Name, Face);
    }

    public void Register(string familyName, FontFace Face)
    {
        if (!_fonts.TryGetValue(familyName, out var list))
        {
            list = new List<FontFaceEntry>();
            _fonts[familyName] = list;
        }

        using var stream = Face.Loader.GetStream();

        var system = new FontSystem();
        system.AddFont(stream);

        list.Add(new FontFaceEntry(Face, system));
    }

    public IEnumerable<string> GetFamilies() => _fonts.Keys;

    public FontFamily? GetFamily(string name)
    {
        if (_fonts.TryGetValue(name, out var list))
            return new FontFamily(name, list.Select(x => x.Face).ToArray());

        return null;
    }

    public IEnumerable<FontFace> GetFaces()
    {
        foreach (var entries in _fonts.Values)
        {
            foreach (var entry in entries)
                yield return entry.Face;
        }
    }

    private FontFace ResolveVariant(Font font)
    {
        if (!_fonts.TryGetValue(font.Name, out var Faces))
            throw new InvalidOperationException($"Font family not registered: {font.Name}");

        var match = Faces.FirstOrDefault(v =>
            v.Face.WeightDefinition == font.Weight &&
            v.Face.StyleDefinition == font.Style);

        if (match == null)
            throw new InvalidOperationException(
                $"FontFace not found: {font.Name} [{font.Weight}, {font.Style}]");

        return match.Face;
    }

    internal SpriteFontBase GetSpriteFont(FontFace Face, float size)
    {
        var key = (Face, size);

        if (_cache.TryGetValue(key, out var cached))
            return cached;

        FontFaceEntry? entryFound = null;

        foreach (var entryList in _fonts.Values)
        {
            foreach(var entry in entryList)
            {
                if (entry.Face == Face)
                {
                    entryFound = entry;
                    break;
                }
            }

            if (entryFound != null)
                break;
        }

        if (entryFound == null)
            throw new InvalidOperationException($"FontFace not found in registry: {Face.Name}");

        var font = entryFound.System.GetFont(size);

        _cache[key] = font;

        return font;
    }

    internal SpriteFontBase GetOrCreateSpriteFont(Font font)
    {
        var Face = ResolveVariant(font);

        return GetSpriteFont(Face, font.Size);
    }
}