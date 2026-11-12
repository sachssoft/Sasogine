using System;
using System.IO;

namespace Sachssoft.Engine.Graphics.Text;

/// <summary>Provides lazy access to the Noto faces embedded in Sasogine.</summary>
public static class BuiltInFonts
{
    private const string Prefix = "Sachssoft.Engine.Graphics.Text.Fonts.";
    private static readonly Lazy<FontFace> SansRegular = Face("Noto Sans", "NotoSans-Regular");
    private static readonly Lazy<FontFace> SansBold = Face("Noto Sans", "NotoSans-Bold", FontWeight.Bold);
    private static readonly Lazy<FontFace> SansItalic = Face("Noto Sans", "NotoSans-Italic", style: FontStyle.Italic);
    private static readonly Lazy<FontFace> SansBoldItalic = Face("Noto Sans", "NotoSans-BoldItalic", FontWeight.Bold, FontStyle.Italic);
    private static readonly Lazy<FontFace> SerifRegular = Face("Noto Serif", "NotoSerif-Regular");
    private static readonly Lazy<FontFace> SerifBold = Face("Noto Serif", "NotoSerif-Bold", FontWeight.Bold);
    private static readonly Lazy<FontFace> SerifItalic = Face("Noto Serif", "NotoSerif-Italic", style: FontStyle.Italic);
    private static readonly Lazy<FontFace> SerifBoldItalic = Face("Noto Serif", "NotoSerif-BoldItalic", FontWeight.Bold, FontStyle.Italic);
    private static readonly Lazy<FontFace> MonoRegular = Face("Noto Sans Mono", "NotoSansMono-Regular");
    private static readonly Lazy<FontFace> MonoBold = Face("Noto Sans Mono", "NotoSansMono-Bold", FontWeight.Bold);

    /// <summary>Gets the default logical size used by the UI.</summary>
    public const float DefaultSize = 16f;

    /// <summary>Gets a sized built-in font. Mono uses its upright regular face when italic is requested.</summary>
    /// <param name="family">The embedded font family.</param>
    /// <param name="size">The positive logical font size.</param>
    /// <param name="style">The requested posture.</param>
    /// <param name="weight">The requested weight.</param>
    /// <param name="backend">The optional backend that resolves the sized font.</param>
    /// <returns>The cached font for the resolved face and size.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The size, family, weight, or style is invalid.</exception>
    public static IFont GetFont(BuiltInFont family, float size, FontStyle style = FontStyle.Normal,
        FontWeight weight = FontWeight.Normal, IFontBackend? backend = null)
    {
        if (!float.IsFinite(size) || size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        FontFace face = GetFace(family, weight, style);
        return backend is null ? face.GetFont(size) : backend.GetFont(face, size);
    }

    internal static FontFace GetFace(BuiltInFont family, FontWeight weight, FontStyle style)
    {
        if (weight is not (FontWeight.Normal or FontWeight.Bold))
            throw new ArgumentOutOfRangeException(nameof(weight));
        if (style is not (FontStyle.Normal or FontStyle.Italic))
            throw new ArgumentOutOfRangeException(nameof(style));

        bool bold = weight == FontWeight.Bold;
        bool italic = style == FontStyle.Italic;
        return family switch
        {
            BuiltInFont.Sans => (italic ? (bold ? SansBoldItalic : SansItalic) : (bold ? SansBold : SansRegular)).Value,
            BuiltInFont.Serif => (italic ? (bold ? SerifBoldItalic : SerifItalic) : (bold ? SerifBold : SerifRegular)).Value,
            BuiltInFont.Mono => (bold && !italic ? MonoBold : MonoRegular).Value,
            _ => throw new ArgumentOutOfRangeException(nameof(family))
        };
    }

    internal static BuiltInFont? FindFamily(string name)
    {
        if (name.Equals("Noto Sans", StringComparison.OrdinalIgnoreCase))
            return BuiltInFont.Sans;
        if (name.Equals("Noto Serif", StringComparison.OrdinalIgnoreCase))
            return BuiltInFont.Serif;
        return name.Equals("Noto Sans Mono", StringComparison.OrdinalIgnoreCase) ? BuiltInFont.Mono : null;
    }

    private static Lazy<FontFace> Face(string family, string fileName, FontWeight weight = FontWeight.Normal,
        FontStyle style = FontStyle.Normal) => new(() =>
    {
        string name = Prefix + fileName + ".ttf";
        using Stream stream = typeof(BuiltInFonts).Assembly.GetManifestResourceStream(name)
            ?? throw new FileNotFoundException("Embedded font resource was not found.", name);
        byte[] bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        return new FontFace(bytes, family, weight, style);
    });
}
