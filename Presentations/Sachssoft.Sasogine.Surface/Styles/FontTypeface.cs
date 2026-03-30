using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Styles
{
    /// <summary>
    /// Represents a font family with multiple font faces (weight + style variants).
    /// </summary>
    public class FontTypeface
    {
        public static readonly FontTypeface Noto =
            new FontTypeface("Noto", [
                new FontVariant(FontResourceLoader.LoadFontSystem("noto/notosans-regular.ttf")) {
                    Weight = FontWeight.Normal,
                    Style = FontStyle.Normal
                },
                new FontVariant(FontResourceLoader.LoadFontSystem("noto/notosans-italic.ttf")) {
                    Weight = FontWeight.Normal,
                    Style = FontStyle.Italic
                },
                new FontVariant(FontResourceLoader.LoadFontSystem("noto/notosans-bold.ttf")) {
                    Weight = FontWeight.Bold,
                    Style = FontStyle.Normal
                },
                new FontVariant(FontResourceLoader.LoadFontSystem("noto/notosans-bold-italic.ttf")) {
                    Weight = FontWeight.Bold,
                    Style = FontStyle.Italic
                }
            ]);

        public static readonly FontTypeface Default = Noto;

        private readonly List<FontVariant> _variants = new();

        public FontTypeface(string name, FontVariant[] faces)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            if (faces.Length == 0)
                throw new ArgumentException("No faces");

            foreach (var face in faces)
            {
                if (_variants.Any(f => f.Weight == face.Weight && f.Style == face.Style))
                {
                    // Already exists, ignore
                    continue;
                }

                _variants.Add(face);
            }
        }

        public string Name { get; }

        public FontVariant GetVariant(FontWeight weight, FontStyle style)
        {
            // Versuche exakt zu finden
            var exact = _variants.FirstOrDefault(f => f.Weight == weight && f.Style == style);
            if (exact != null)
                return exact;

            // Fallback auf Normal-Weight, gleiche Style
            var weightFallback = _variants.FirstOrDefault(f => f.Weight == FontWeight.Normal && f.Style == style);
            if (weightFallback != null)
                return weightFallback;

            // Fallback auf Normal-Style, gleiche Weight
            var styleFallback = _variants.FirstOrDefault(f => f.Weight == weight && f.Style == FontStyle.Normal);
            if (styleFallback != null)
                return styleFallback;

            // Letzter Versuch: beides Normal
            var normal = _variants.FirstOrDefault(f => f.Weight == FontWeight.Normal && f.Style == FontStyle.Normal);
            if (normal != null)
                return normal;

            // Wenn nichts gefunden, Exception
            throw new InvalidOperationException($"No font variant found for weight {weight} and style {style}.");
        }


        /// <summary>
        /// Retrieves all registered font faces.
        /// </summary>
        public IReadOnlyList<FontVariant> Variants => _variants;
    }
}
