using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace Sachssoft.Engine.Resources.Importers.Internal;

/// <summary>Imports sprite regions from a single TextureAtlas or an ordered collection of atlas pages.</summary>
internal sealed class XmlFrameSetImporter : FrameSetImporter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlFrameSetImporter"/> class.
    /// </summary>
    /// <param name="resourceSource">The resource source containing the frame set document.</param>
    public XmlFrameSetImporter(ResourceSourceBase resourceSource)
        : base(resourceSource)
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<FrameSetEntry> OnImporting(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var document = XDocument.Load(stream);

        var root = document.Root
            ?? throw new InvalidDataException(
                "The XML document does not contain a root element.");

        IEnumerable<XElement> atlases = root.Elements().Where(element => element.Name.LocalName == "TextureAtlas");
        if (!atlases.Any()) atlases = new[] { root };
        int page = 0;
        foreach (XElement atlas in atlases)
        {
            int textureIndex = GetTextureIndex(atlas, page++);
            foreach (var element in atlas.Elements().Where(element => element.Name.LocalName == "sprite"))
            {
                yield return new FrameSetEntry
                {
                    TextureIndex = GetTextureIndex(element, textureIndex),
                    Name = element.Attribute("n")?.Value ?? string.Empty,
                    X = GetInt(element, "x"),
                    Y = GetInt(element, "y"),
                    Width = GetInt(element, "w"),
                    Height = GetInt(element, "h"),
                    PivotX = GetFloat(element, "pX"),
                    PivotY = GetFloat(element, "pY"),
                    SpriteSourceX = GetInt(element, "oX"),
                    SpriteSourceY = GetInt(element, "oY"),
                    SpriteSourceWidth = GetInt(element, "w"),
                    SpriteSourceHeight = GetInt(element, "h"),
                    SourceWidth = GetInt(element, "oW"),
                    SourceHeight = GetInt(element, "oH"),
                    IsRotated = string.Equals(
                        element.Attribute("r")?.Value,
                        "y",
                        StringComparison.OrdinalIgnoreCase),
                    IsTrimmed = GetInt(element, "oX") != 0 ||
                                GetInt(element, "oY") != 0 ||
                                GetInt(element, "w") != GetInt(element, "oW") ||
                                GetInt(element, "h") != GetInt(element, "oH")
                };
            }
        }
    }

    /// <summary>Reads an optional explicit page index, falling back to the enclosing atlas order.</summary>
    /// <param name="element">The atlas or sprite element.</param>
    /// <param name="fallback">The enclosing page index.</param>
    /// <returns>A nonnegative atlas page index.</returns>
    private static int GetTextureIndex(XElement element, int fallback)
    {
        XAttribute? attribute = element.Attribute("textureIndex") ?? element.Attribute("page");
        if (attribute is null) return fallback;
        if (!int.TryParse(attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index) || index < 0)
            throw new InvalidDataException("An atlas texture index must be a nonnegative integer.");
        return index;
    }

    /// <summary>Reads optional integer atlas metadata using invariant culture.</summary>
    /// <param name="element">The sprite element.</param>
    /// <param name="name">The external attribute name.</param>
    /// <returns>The integer value, or zero when omitted.</returns>
    private static int GetInt(XElement element, string name)
    {
        return int.TryParse(
            element.Attribute(name)?.Value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : 0;
    }

    /// <summary>Reads optional floating-point atlas metadata using invariant culture.</summary>
    /// <param name="element">The sprite element.</param>
    /// <param name="name">The external attribute name.</param>
    /// <returns>The floating-point value, or zero when omitted.</returns>
    private static float GetFloat(XElement element, string name)
    {
        return float.TryParse(
            element.Attribute(name)?.Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : 0f;
    }
}
