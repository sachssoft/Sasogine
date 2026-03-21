using FontStashSharp;
using nkast.Aether.Physics2D.Common;
using Sachssoft.Sasogine.Surface.Styles;
using System;

namespace Sachssoft.Sasogine.Surface.Visuals
{
    public record Font : IStyleFactory<Font>
    {
        public static Font Default = new Font()
        {
            Typeface = "Noto"
        };

        public string? Id { get; init; }

        /// <summary>Font family name (e.g., "Arial").</summary>
        public required string Typeface { get; init; }

        /// <summary>Font size in points.</summary>
        public float Size { get; init; } = 12f;

        /// <summary>Font weight (e.g., Normal, Bold).</summary>
        public FontWeight Weight { get; init; } = FontWeight.Normal;

        /// <summary>Font style (Normal, Italic).</summary>
        public FontStyle Style { get; init; } = FontStyle.Normal;

        /// <summary>Optional text decorations (Underline, StrikeThrough, None).</summary>
        public TextDecoration Decoration { get; init; } = TextDecoration.None;

        public SpriteFontBase GetSpriteFont() => GetSpriteFont(Stylesheet.Current);

        public SpriteFontBase GetSpriteFont(Stylesheet sheet)
        {
            if (!sheet.Typefaces.TryGetValue(Typeface, out var typeface))
                throw new InvalidOperationException($"Typeface '{Typeface}' not found.");

            var variant = typeface.GetVariant(Weight, Style)
                          ?? throw new InvalidOperationException($"Variant not found for weight '{Weight}' and style '{Style}'.");

            return variant.GetFont(Size);
        }

        #region IStyleFactory

        static string IStyleFactory<Font>.PrefixName => nameof(Font);

        static Font IStyleFactory<Font>.Create(StyleFactoryContext<Font> context)
        {
            var fontTypeface = "Noto";
            var fontSize = 12f;
            var fontWeight = FontWeight.Normal;
            var fontStyle = FontStyle.Normal;
            var decoration = TextDecoration.None;

            foreach (var property in context.Properties)
            {
                switch (property)
                {
                    case nameof(Typeface):
                        fontTypeface = context.Style.Stylesheet.FindTypefaceOrDefault(context.GetValue(property).RawValue).Name;
                        break;
                    case nameof(Size):
                        fontSize = context.GetValue(property).ConvertTo<float>();
                        break;
                    case nameof(Weight):
                        fontWeight = context.GetValue(property).ConvertTo<FontWeight>();
                        break;
                    case nameof(Style):
                        fontStyle = context.GetValue(property).ConvertTo<FontStyle>();
                        break;
                    case nameof(Decoration):
                        decoration = context.GetValue(property).ConvertTo<TextDecoration>();
                        break;
                }
            }

            return new Font()
            {
                Typeface = fontTypeface,
                Size = fontSize,
                Weight = fontWeight,
                Style = fontStyle,
                Decoration = decoration,
            };
        }

        #endregion
    }

}
