using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Sachssoft.Sasogine.Experimental.Resources.Markup.Internal
{
    internal sealed class JsonFrameSetLoader : FrameSetLoader
    {
        public JsonFrameSetLoader(ResourceSourceBase resourceSource)
            : base(resourceSource)
        {
        }

        protected override IEnumerable<FrameSetEntry> OnLoading(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            using var document = JsonDocument.Parse(stream);

            var root = document.RootElement;

            if (!root.TryGetProperty("frames", out var frames))
                throw new InvalidDataException(
                    "The JSON document does not contain a 'frames' property.");

            if (frames.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException(
                    "The 'frames' property must be an array.");

            foreach (var frame in frames.EnumerateArray())
            {
                var frameData = GetRequiredObject(frame, "frame");
                var spriteSourceSize = GetObject(frame, "spriteSourceSize");
                var sourceSize = GetObject(frame, "sourceSize");
                var pivot = GetObject(frame, "pivot");

                yield return new FrameSetEntry
                {
                    Name = GetRequiredString(frame, "filename"),

                    X = GetRequiredInt(frameData, "x"),
                    Y = GetRequiredInt(frameData, "y"),
                    Width = GetRequiredInt(frameData, "w"),
                    Height = GetRequiredInt(frameData, "h"),

                    IsRotated = GetBool(frame, "rotated"),
                    IsTrimmed = GetBool(frame, "trimmed"),

                    SpriteSourceX = GetInt(spriteSourceSize, "x"),
                    SpriteSourceY = GetInt(spriteSourceSize, "y"),
                    SpriteSourceWidth = GetInt(spriteSourceSize, "w"),
                    SpriteSourceHeight = GetInt(spriteSourceSize, "h"),

                    SourceWidth = GetInt(sourceSize, "w"),
                    SourceHeight = GetInt(sourceSize, "h"),

                    PivotX = GetFloat(pivot, "x"),
                    PivotY = GetFloat(pivot, "y")
                };
            }
        }

        private static JsonElement GetRequiredObject(
            JsonElement element,
            string name)
        {
            if (!element.TryGetProperty(name, out var property) ||
                property.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException(
                    $"Missing or invalid object property '{name}'.");
            }

            return property;
        }

        private static JsonElement? GetObject(
            JsonElement element,
            string name)
        {
            if (!element.TryGetProperty(name, out var property) ||
                property.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return property;
        }

        private static string GetRequiredString(
            JsonElement element,
            string name)
        {
            if (!element.TryGetProperty(name, out var property) ||
                property.ValueKind != JsonValueKind.String)
            {
                throw new InvalidDataException(
                    $"Missing or invalid '{name}' property.");
            }

            return property.GetString()
                ?? throw new InvalidDataException(
                    $"The '{name}' property must not be null.");
        }

        private static int GetRequiredInt(
            JsonElement element,
            string name)
        {
            if (!element.TryGetProperty(name, out var property) ||
                !property.TryGetInt32(out var value))
            {
                throw new InvalidDataException(
                    $"Missing or invalid integer property '{name}'.");
            }

            return value;
        }

        private static int GetInt(
            JsonElement? element,
            string name)
        {
            if (!element.HasValue)
                return 0;

            return element.Value.TryGetProperty(name, out var property) &&
                   property.TryGetInt32(out var value)
                ? value
                : 0;
        }

        private static float GetFloat(
            JsonElement? element,
            string name)
        {
            if (!element.HasValue)
                return 0f;

            return element.Value.TryGetProperty(name, out var property) &&
                   property.TryGetSingle(out var value)
                ? value
                : 0f;
        }

        private static bool GetBool(
            JsonElement element,
            string name)
        {
            return element.TryGetProperty(name, out var property) &&
                   property.ValueKind == JsonValueKind.True;
        }
    }
}