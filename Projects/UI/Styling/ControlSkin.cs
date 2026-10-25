using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Sachssoft.Engine.Controls.Styling;
/// <summary>
/// Loads known XML fields and style keys explicitly. Owns its texture only when loaded through Load.
/// </summary>
public sealed class ControlSkin : IDisposable
{
    private readonly Dictionary<string, SkinRegion> _regions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _parts = new(StringComparer.Ordinal);
    private readonly List<(string Key, string? Base, List<(string Key, string Value)> Values)> _styles = new();
    private readonly Dictionary<string, Color> _palette = new(StringComparer.Ordinal);
    private readonly int _width, _height;
    private bool _ownsTexture, _disposed;
    private ControlSkin(int width, int height)
    {
        _width = width;
        _height = height;
    }

    /// <summary>
    /// Gets the optional atlas texture; null permits headless validation.
    /// </summary>
    public Texture2D? Texture { get; private set; }
    /// <summary>
    /// Gets the region count.
    /// </summary>
    public int RegionCount => _regions.Count;

    /// <summary>
    /// Gets a validated atlas region.
    /// </summary>
    public SkinRegion this[string key] => _regions[key];
    /// <summary>
    /// Gets the name declared by the skin.
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// Gets the validated atlas filename declared by the skin.
    /// </summary>
    public string AtlasFileName { get; private set; } = string.Empty;
    /// <summary>
    /// Gets the validated texture filename declared by the atlas.
    /// </summary>
    public string TextureFileName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a named palette color.
    /// </summary>
    public Color GetColor(string name) => _palette[name];
    /// <summary>
    /// Attempts to resolve a named palette color.
    /// </summary>
    public bool TryGetColor(string name, out Color color) => _palette.TryGetValue(name, out color);
    /// <summary>
    /// Loads the single skin manifest in a folder using premultiplied atlas pixels.
    /// </summary>
    public static ControlSkin Load(GraphicsDevice device, string directory)
    {
        ArgumentNullException.ThrowIfNull(device);
        string[] manifests = Directory.GetFiles(directory, "*.skin.xml");
        if (manifests.Length != 1)
            throw new InvalidDataException("The folder must contain exactly one skin manifest. Use the explicit filename overload for multiple manifests.");
        return Load(device, directory, Path.GetFileName(manifests[0]));
    }

    /// <summary>
    /// Loads a selected skin manifest and its declared atlas and texture from one folder.
    /// </summary>
    public static ControlSkin Load(GraphicsDevice device, string directory, string skinFileName)
    {
        ArgumentNullException.ThrowIfNull(device);
        XElement document;
        using (var stream = File.OpenRead(Path.Combine(directory, SafeFileName(skinFileName))))
            document = ReadRoot(stream);
        string atlasName = SafeFileName(Required(document, "atlas"));
        XElement atlas;
        using (var stream = File.OpenRead(Path.Combine(directory, atlasName)))
            atlas = ReadRoot(stream);
        ControlSkin skin = ReadCore(atlas, document);
        try
        {
            using var textureStream = File.OpenRead(Path.Combine(directory, skin.TextureFileName));
            skin.AttachTexture(Texture2D.FromStream(device, textureStream), true);
            return skin;
        }
        catch
        {
            skin.Dispose();
            throw;
        }
    }

    private static string SafeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || name.Contains('/') || name.Contains('\\') || name.Contains(':') || name.EndsWith(' ') || name.EndsWith('.') || Path.IsPathRooted(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidDataException("Skin resources must use filenames inside their skin folder.");
        return name;
    }

    private static XElement ReadRoot(Stream stream)
    {
        using XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 4_000_000, CloseInput = false });
        return XDocument.Load(reader).Root ?? throw new InvalidDataException("XML root is missing.");
    }

    /// <summary>
    /// Reads explicit skin and atlas metadata with DTDs disabled.
    /// </summary>
    public static ControlSkin Read(Stream skinStream, Stream atlasStream) => ReadCore(ReadRoot(atlasStream), ReadRoot(skinStream));
    private static ControlSkin ReadCore(XElement atlas, XElement document)
    {
        if (atlas.Name != "TextureAtlas" || document.Name != "UiSkin")
            throw new InvalidDataException("Unexpected skin or atlas root.");
        int width = Int(atlas, "width"), height = Int(atlas, "height");
        if (width <= 0 || height <= 0)
            throw new InvalidDataException("Invalid atlas dimensions.");
        var skin = new ControlSkin(width, height)
        {
            Name = Required(document, "name"),
            AtlasFileName = SafeFileName(Required(document, "atlas")),
            TextureFileName = SafeFileName(Required(atlas, "imagePath"))
        };
        var frames = new Dictionary<string, Rectangle>(StringComparer.Ordinal);
        foreach (XElement entry in atlas.Elements("sprite"))
        {
            Rectangle bounds = new(Int(entry, "x"), Int(entry, "y"), Int(entry, "w"), Int(entry, "h"));
            if (bounds.X < 0 || bounds.Y < 0 || bounds.Width <= 0 || bounds.Height <= 0 || (long)bounds.X + bounds.Width > width || (long)bounds.Y + bounds.Height > height || (string? )entry.Attribute("r") == "y")
                throw new InvalidDataException("Invalid or rotated atlas region.");
            frames.Add(Required(entry, "n"), bounds);
        }

        foreach (XElement color in document.Element("Palette")?.Elements("Color") ?? Array.Empty<XElement>())
            skin._palette.Add(Required(color, "name"), ParseColor(Required(color, "value")));
        foreach (XElement region in document.Element("Regions")?.Elements("Region") ?? Array.Empty<XElement>())
        {
            Rectangle bounds = frames[Required(region, "frame")];
            string[] slices = Required(region, "slice").Split(',');
            if (slices.Length != 4)
                throw new InvalidDataException("Invalid slice definition.");
            int l = int.Parse(slices[0], CultureInfo.InvariantCulture), t = int.Parse(slices[1], CultureInfo.InvariantCulture), r = int.Parse(slices[2], CultureInfo.InvariantCulture), b = int.Parse(slices[3], CultureInfo.InvariantCulture);
            string mode = Required(region, "mode");
            if (l < 0 || t < 0 || r < 0 || b < 0 || (long)l + r >= bounds.Width || (long)t + b >= bounds.Height || mode is not ("Fixed" or "Stretch" or "Tile" or "NineSlice"))
                throw new InvalidDataException("Invalid region scaling.");
            skin._regions.Add(Required(region, "key"), new SkinRegion(bounds, mode, new PixelInsets2(l, t, r, b)));
        }

        foreach (XElement style in document.Element("Styles")?.Elements("Style") ?? Array.Empty<XElement>())
        {
            string key = Required(style, "key");
            var parts = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            var values = new List<(string, string)>();
            foreach (XElement value in style.Elements("Value"))
                values.Add((Required(value, "property"), Required(value, "value")));
            foreach (XElement part in style.Elements("Part"))
            {
                var states = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (XElement state in part.Elements("State"))
                {
                    string resource = Required(state, "region");
                    if (!skin._regions.ContainsKey(resource))
                        throw new InvalidDataException("Unknown skin region.");
                    states.Add(Required(state, "name"), resource);
                }

                parts.Add(Required(part, "name"), states);
            }

            skin._parts.Add(key, parts);
            skin._styles.Add((key, (string? )style.Attribute("basedOn"), values));
        }

        var graph = new StyleRegistry();
        foreach (var style in skin._styles)
            graph.Register(style.Key, new Style { BasedOn = style.Base });
        foreach (var style in skin._styles)
            graph.Validate(style.Key);
        return skin;
    }

    /// <summary>
    /// Attaches a matching atlas. A borrowed texture remains owned by the caller.
    /// </summary>
    public void AttachTexture(Texture2D texture, bool ownsTexture = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(texture);
        if (texture.Width != _width || texture.Height != _height)
            throw new ArgumentException("Texture dimensions do not match the atlas.", nameof(texture));
        if (_ownsTexture)
            Texture?.Dispose();
        Texture = texture;
        _ownsTexture = ownsTexture;
    }

    /// <summary>
    /// Applies supported typed style values while preserving explicit standard settings.
    /// </summary>
    public void Apply(UIRegistry registry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(registry);
        foreach (var previous in registry.SkinStyles)
            if (registry.Styles.TryGet(previous.Key, out Style? current))
            {
                Style? restored = previous.Value.Restore(current!);
                if (restored is null)
                    registry.Styles.RemoveSkinStyle(previous.Key);
                else
                    registry.Styles.Set(previous.Key, restored);
            }

        registry.SkinStyles.Clear();
        foreach (var item in _styles)
        {
            var style = new Style
            {
                BasedOn = item.Base
            };
            if (registry.Styles.TryGet(item.Key, out Style? existing))
                foreach (var value in existing!.Values)
                    style[value.Key] = value.Value;
            foreach (var value in item.Values)
                ApplyValue(style, value.Key, value.Value);
            registry.SkinStyles[item.Key] = new SkinStyleState(existing, style);
            registry.Styles.Set(item.Key, style);
        }

        registry.SkinColors.Clear();
        string[] colors =
        {
            "Background",
            "HoverBackground",
            "PressedBackground",
            "DisabledBackground",
            "FocusColor",
            "BorderColor"
        };
        foreach (string name in registry.Styles.Keys)
        {
            var values = new Dictionary<string, Color>(StringComparer.Ordinal);
            foreach (string key in colors)
                values[key] = registry.Styles.TryResolve(name, key, out string? text) ? registry.Converters.Convert<Color>("Color", text!) : Color.Transparent;
            registry.SkinColors[name] = values;
        }

        registry.Skin = this;
    }

    private readonly Dictionary<(string Id, string Kind, string Part, string State), SkinRegion?> _resolution = new();
    private int _resolutionRevision = -1;
    private StyleRegistry? _resolutionStyles;
    internal bool Find(Control control, string part, string state, StyleRegistry styles, out SkinRegion? region)
    {
        if (!ReferenceEquals(_resolutionStyles, styles) || _resolutionRevision != styles.Revision)
        {
            _resolution.Clear();
            _resolutionRevision = styles.Revision;
            _resolutionStyles = styles;
        }

        var key = (control.StyleId, control.StandardStyleName, part, state);
        if (_resolution.TryGetValue(key, out region))
            return region is not null;
        bool found = FindUncached(control, part, state, styles, out region);
        _resolution[key] = region;
        return found;
    }

    private bool FindUncached(Control control, string part, string state, StyleRegistry styles, out SkinRegion? region)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var candidates = new List<string>
        {
            state
        };
        int dot = state.IndexOf('.');
        if (dot >= 0)
        {
            candidates.Add(state[..dot] + ".Normal");
            candidates.Add(state[(dot + 1)..]);
        }

        if (!candidates.Contains("Normal"))
            candidates.Add("Normal");
        foreach (string candidate in candidates)
            foreach (string initial in new[]
            {
                control.StyleId,
                control.StandardStyleName
            }

            )
            {
                string? key = initial.Length == 0 ? null : initial;
                int depth = 0;
                while (key is not null)
                {
                    if (++depth > styles.Count + 1)
                        throw new InvalidOperationException("Cyclic style chain.");
                    if (_parts.TryGetValue(key, out var parts) && parts.TryGetValue(part, out var states) && states.TryGetValue(candidate, out string? name))
                    {
                        region = _regions[name];
                        return true;
                    }

                    key = styles[key].BasedOn;
                }
            }

        region = null;
        return false;
    }

    private void ApplyValue(Style style, string key, string text)
    {
        string? mapped = key switch
        {
            "ContentPadding" => "Padding",
            "Thickness" => "ScrollBarThickness",
            "TileSpacing" => "Spacing",
            "Background" or "BorderColor" or "BorderThickness" or "HoverBackground" or "PressedBackground" or "DisabledBackground" => key,
            "RepeatDelay" or "RepeatInterval" => key,
            nameof(Hyperlink.HoverTextColor) or nameof(Hyperlink.PressedTextColor) or nameof(Hyperlink.VisitedTextColor) or nameof(Hyperlink.Underline) => key,
            "TextColor" or "DisabledTextColor" or "Padding" or "MinWidth" or "MinHeight" or "Height" or "MinimumThumbExtent" or "TrackThickness" or "ItemHeight" or "TitlebarHeight" or "Spacing" or "TextSpacing" or "HeaderHeight" or "KeywordColor" or "StringColor" or "CommentColor" or "NumberColor" or "Language" or "Kind" or "AutoScale" or "LineThickness" or "IsExpanded" or "Minimum" or "Maximum" or "Step" or "ButtonWidth" or "ThumbWidth" or "DragThreshold" => key,
            _ => null
        };
        if (mapped is null)
            return;
        style[mapped] = _palette.TryGetValue(text, out Color c) ? $"{c.R},{c.G},{c.B},{c.A}" : text;
    }

    internal bool FindStyle(string initial, string part, string state, StyleRegistry styles, out SkinRegion? region)
    {
        for (int candidateIndex = 0; candidateIndex < 2; candidateIndex++)
        {
            string candidate = candidateIndex == 0 ? state : "Normal";
            string? key = initial;
            int depth = 0;
            while (key is not null)
            {
                if (++depth > styles.Count + 1)
                    throw new InvalidOperationException("Cyclic style chain.");
                if (_parts.TryGetValue(key, out var parts) && parts.TryGetValue(part, out var states) && states.TryGetValue(candidate, out string? name))
                {
                    region = _regions[name];
                    return true;
                }

                key = styles[key].BasedOn;
            }
        }

        region = null;
        return false;
    }

    private static string Required(XElement node, string key) => (string? )node.Attribute(key) ?? throw new InvalidDataException($"Missing XML attribute '{key}'.");
    private static int Int(XElement node, string key) => int.Parse(Required(node, key), CultureInfo.InvariantCulture);
    internal static Color ParseColor(string text)
    {
        text = text.TrimStart('#');
        if (text.Length is not (6 or 8))
            throw new FormatException("Use RRGGBB or RRGGBBAA.");
        byte R(int i) => byte.Parse(text.AsSpan(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return Color.FromNonPremultiplied(R(0), R(2), R(4), text.Length == 8 ? R(6) : 255);
    }

    /// <summary>
    /// Disposes an owned atlas texture.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        if (_ownsTexture)
            Texture?.Dispose();
        Texture = null;
        _disposed = true;
    }
}
