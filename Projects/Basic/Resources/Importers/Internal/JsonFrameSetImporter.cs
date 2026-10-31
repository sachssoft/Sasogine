using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Sachssoft.Engine.Resources.Importers.Internal;

/// <summary>Imports array or hash frames from single-page and multi-page JSON atlases.</summary>
internal sealed class JsonFrameSetImporter : FrameSetImporter
{
    /// <summary>Creates a JSON atlas importer.</summary>
    /// <param name="resourceSource">The document source.</param>
    public JsonFrameSetImporter(ResourceSourceBase resourceSource) : base(resourceSource)
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<FrameSetEntry> OnImporting(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var document = JsonDocument.Parse(stream);
        JsonElement root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            int index = 0;
            foreach (JsonElement page in root.EnumerateArray())
                foreach (FrameSetEntry entry in ImportPage(page, index++))
                    yield return entry;
        }
        else if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("textures", out JsonElement textures))
        {
            if (textures.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("The 'textures' property must be an array.");
            int index = 0;
            foreach (JsonElement page in textures.EnumerateArray())
                foreach (FrameSetEntry entry in ImportPage(page, index++))
                    yield return entry;
        }
        else
        {
            foreach (FrameSetEntry entry in ImportPage(root, 0))
                yield return entry;
        }
    }

    /// <summary>Imports one atlas page, preserving explicit frame-level page assignments.</summary>
    /// <param name="page">The page object containing array or hash frames.</param>
    /// <param name="defaultIndex">The page's position in the document.</param>
    /// <returns>The page's frame entries.</returns>
    private static IEnumerable<FrameSetEntry> ImportPage(JsonElement page, int defaultIndex)
    {
        if (page.ValueKind != JsonValueKind.Object || !page.TryGetProperty("frames", out JsonElement frames))
            throw new InvalidDataException("Each atlas page must contain a 'frames' property.");
        int textureIndex = GetTextureIndex(page, defaultIndex);
        if (frames.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement frame in frames.EnumerateArray())
                yield return ReadFrame(frame, textureIndex, null);
        }
        else if (frames.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty frame in frames.EnumerateObject())
                yield return ReadFrame(frame.Value, textureIndex, frame.Name);
        }
        else
            throw new InvalidDataException("The 'frames' property must be an array or an object.");
    }

    /// <summary>Reads a complete frame entry without loading or taking ownership of any texture.</summary>
    /// <param name="frame">The frame object.</param>
    /// <param name="textureIndex">The enclosing atlas page.</param>
    /// <param name="name">The hash key, or null to read the array entry's filename.</param>
    /// <returns>The imported frame and source metadata.</returns>
    private static FrameSetEntry ReadFrame(JsonElement frame, int textureIndex, string? name)
    {
        var bounds = GetRequiredObject(frame, "frame");
        var spriteSource = GetObject(frame, "spriteSourceSize");
        var source = GetObject(frame, "sourceSize");
        var pivot = GetObject(frame, "pivot");
        return new FrameSetEntry
        {
            Name = name ?? GetRequiredString(frame, "filename"),
            TextureIndex = GetTextureIndex(frame, textureIndex),
            X = GetRequiredInt(bounds, "x"),
            Y = GetRequiredInt(bounds, "y"),
            Width = GetRequiredInt(bounds, "w"),
            Height = GetRequiredInt(bounds, "h"),
            IsRotated = GetBool(frame, "rotated"),
            IsTrimmed = GetBool(frame, "trimmed"),
            SpriteSourceX = GetInt(spriteSource, "x"),
            SpriteSourceY = GetInt(spriteSource, "y"),
            SpriteSourceWidth = GetInt(spriteSource, "w"),
            SpriteSourceHeight = GetInt(spriteSource, "h"),
            SourceWidth = GetInt(source, "w"),
            SourceHeight = GetInt(source, "h"),
            PivotX = GetFloat(pivot, "x"),
            PivotY = GetFloat(pivot, "y")
        };
    }

    /// <summary>Reads an explicit nonnegative textureIndex or page, or uses the enclosing page index.</summary>
    /// <param name="element">The atlas or frame object.</param>
    /// <param name="fallback">The enclosing page index.</param>
    /// <returns>The validated page index.</returns>
    private static int GetTextureIndex(JsonElement element, int fallback)
    {
        if (!element.TryGetProperty("textureIndex", out JsonElement value) && !element.TryGetProperty("page", out value))
            return fallback;
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int index) || index < 0)
            throw new InvalidDataException("An atlas texture index must be a nonnegative integer.");
        return index;
    }

    /// <summary>Reads an object property required by the atlas format.</summary>
    /// <param name="element">The containing object.</param>
    /// <param name="name">The external property name.</param>
    /// <returns>The required object.</returns>
    private static JsonElement GetRequiredObject(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out JsonElement value) ||
            value.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException($"Missing or invalid object property '{name}'.");
        return value;
    }

    /// <summary>Reads an optional object property.</summary>
    /// <param name="element">The containing object.</param>
    /// <param name="name">The external property name.</param>
    /// <returns>The object, or null when omitted.</returns>
    private static JsonElement? GetObject(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Object ? value : null;

    /// <summary>Reads a required frame name.</summary>
    /// <param name="element">The frame object.</param>
    /// <param name="name">The external property name.</param>
    /// <returns>The non-null string value.</returns>
    private static string GetRequiredString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.String)
            throw new InvalidDataException($"Missing or invalid '{name}' property.");
        return value.GetString() ?? throw new InvalidDataException($"The '{name}' property must not be null.");
    }

    /// <summary>Reads a required pixel coordinate or dimension.</summary>
    /// <param name="element">The bounds object.</param>
    /// <param name="name">The external property name.</param>
    /// <returns>The integer coordinate or dimension.</returns>
    private static int GetRequiredInt(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.Number ||
            !value.TryGetInt32(out int result))
            throw new InvalidDataException($"Missing or invalid integer property '{name}'.");
        return result;
    }

    /// <summary>Reads optional integer source metadata.</summary>
    /// <param name="element">The optional metadata object.</param>
    /// <param name="name">The external property name.</param>
    /// <returns>The integer value, or zero when omitted.</returns>
    private static int GetInt(JsonElement? element, string name) =>
        element.HasValue && element.Value.TryGetProperty(name, out JsonElement value) && value.TryGetInt32(out int result) ? result : 0;

    /// <summary>Reads optional floating-point source metadata.</summary>
    /// <param name="element">The optional metadata object.</param>
    /// <param name="name">The external property name.</param>
    /// <returns>The floating-point value, or zero when omitted.</returns>
    private static float GetFloat(JsonElement? element, string name) =>
        element.HasValue && element.Value.TryGetProperty(name, out JsonElement value) && value.TryGetSingle(out float result) ? result : 0;

    /// <summary>Reads an optional boolean frame flag.</summary>
    /// <param name="element">The frame object.</param>
    /// <param name="name">The external property name.</param>
    /// <returns>Whether the property is explicitly true.</returns>
    private static bool GetBool(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.True;
}