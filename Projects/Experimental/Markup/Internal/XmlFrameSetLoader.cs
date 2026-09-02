using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace Sachssoft.Sasogine.Experimental.Resources.Markup.Internal
{
    internal sealed class XmlFrameSetLoader : FrameSetLoader
    {
        public XmlFrameSetLoader(ResourceSourceBase resourceSource)
            : base(resourceSource)
        {
        }

        protected override IEnumerable<FrameSetEntry> OnLoading(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var document = XDocument.Load(stream);

            var root = document.Root
                ?? throw new InvalidDataException(
                    "The XML document does not contain a root element.");

            foreach (var element in root.Elements("sprite"))
            {
                yield return new FrameSetEntry
                {
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
}