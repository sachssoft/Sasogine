using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace Sachssoft.Sasogine.Markup.Internal
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

            if (!document.RootElement.TryGetProperty(
                    "frames",
                    out var frames))
            {
                throw new InvalidDataException(
                    "The JSON document does not contain a 'frames' property.");
            }

            if (frames.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException(
                    "The 'frames' property must be an array.");
            }

            foreach (var frame in frames.EnumerateArray())
            {
                yield return new FrameSetEntry
                {
                    Name = GetRequiredString(frame, "filename"),

                    X = GetRequiredInt(
                        frame.GetProperty("frame"),
                        "x"),

                    Y = GetRequiredInt(
                        frame.GetProperty("frame"),
                        "y"),

                    Width = GetRequiredInt(
                        frame.GetProperty("frame"),
                        "w"),

                    Height = GetRequiredInt(
                        frame.GetProperty("frame"),
                        "h"),

                    PivotX = GetFloat(
                        frame,
                        "pivot",
                        "x"),

                    PivotY = GetFloat(
                        frame,
                        "pivot",
                        "y"),

                    OffsetX = GetInt(
                        frame,
                        "spriteSourceSize",
                        "x"),

                    OffsetY = GetInt(
                        frame,
                        "spriteSourceSize",
                        "y"),

                    OriginalWidth = GetInt(
                        frame,
                        "sourceSize",
                        "w"),

                    OriginalHeight = GetInt(
                        frame,
                        "sourceSize",
                        "h"),

                    IsRotated = GetBool(
                        frame,
                        "rotated")
                };
            }
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
            JsonElement element,
            string objectName,
            string propertyName)
        {
            if (!element.TryGetProperty(objectName, out var parent) ||
                parent.ValueKind != JsonValueKind.Object)
            {
                return 0;
            }

            return parent.TryGetProperty(propertyName, out var property) &&
                   property.TryGetInt32(out var value)
                ? value
                : 0;
        }

        private static float GetFloat(
            JsonElement element,
            string objectName,
            string propertyName)
        {
            if (!element.TryGetProperty(objectName, out var parent) ||
                parent.ValueKind != JsonValueKind.Object)
            {
                return 0f;
            }

            return parent.TryGetProperty(propertyName, out var property) &&
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