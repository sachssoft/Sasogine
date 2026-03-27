using JetBrains.Annotations;

namespace Box2D;

/// <summary>
/// These colors are used for debug draw and mostly match the named SVG colors.
/// See https://www.rapidtables.com/web/color/index.html
/// https://johndecember.com/html/spec/colorsvg.html
/// https://upload.wikimedia.org/wikipedia/commons/2/2b/SVG_Recognized_color_keyword_names.svg
/// </summary>
/// <remarks>
/// You can cast any uint to HexColor.
/// </remarks>
[PublicAPI]
public enum HexColor : uint
{
    /// <summary>
    /// AliceBlue: a very pale, almost white-blue (#F0F8FF).
    /// </summary>
    AliceBlue = 0xF0F8FF,
    /// <summary>
    /// AntiqueWhite: a soft, creamy off-white (#FAEBD7).
    /// </summary>
    AntiqueWhite = 0xFAEBD7,
    /// <summary>
    /// Aqua: a bright cyan-blue (#00FFFF).
    /// </summary>
    Aqua = 0x00FFFF,
    /// <summary>
    /// Aquamarine: a light greenish-blue (#7FFFD4).
    /// </summary>
    Aquamarine = 0x7FFFD4,
    /// <summary>
    /// Azure: a pale cyan-blue, almost white (#F0FFFF).
    /// </summary>
    Azure = 0xF0FFFF,
    /// <summary>
    /// Beige: a light brownish-yellow (#F5F5DC).
    /// </summary>
    Beige = 0xF5F5DC,
    /// <summary>
    /// Bisque: a soft, warm peach (#FFE4C4).
    /// </summary>
    Bisque = 0xFFE4C4,
    /// <summary>
    /// Black: pure black (#000000).
    /// </summary>
    Black = 0x000000,
    /// <summary>
    /// BlanchedAlmond: a pale almond cream (#FFEBCD).
    /// </summary>
    BlanchedAlmond = 0xFFEBCD,
    /// <summary>
    /// Blue: pure blue (#0000FF).
    /// </summary>
    Blue = 0x0000FF,
    /// <summary>
    /// BlueViolet: a deep violet-blue (#8A2BE2).
    /// </summary>
    BlueViolet = 0x8A2BE2,
    /// <summary>
    /// Brown: a dark reddish-brown (#A52A2A).
    /// </summary>
    Brown = 0xA52A2A,
    /// <summary>
    /// Burlywood: a warm tan (#DEB887).
    /// </summary>
    Burlywood = 0xDEB887,
    /// <summary>
    /// CadetBlue: a muted teal-blue (#5F9EA0).
    /// </summary>
    CadetBlue = 0x5F9EA0,
    /// <summary>
    /// Chartreuse: a vivid yellow-green (#7FFF00).
    /// </summary>
    Chartreuse = 0x7FFF00,
    /// <summary>
    /// Chocolate: a rich, dark brown (#D2691E).
    /// </summary>
    Chocolate = 0xD2691E,
    /// <summary>
    /// Coral: a soft pinkish-orange (#FF7F50).
    /// </summary>
    Coral = 0xFF7F50,
    /// <summary>
    /// CornflowerBlue: a medium slate-blue (#6495ED).
    /// </summary>
    CornflowerBlue = 0x6495ED,
    /// <summary>
    /// Cornsilk: a very pale yellow (#FFF8DC).
    /// </summary>
    Cornsilk = 0xFFF8DC,
    /// <summary>
    /// Crimson: a strong, deep red (#DC143C).
    /// </summary>
    Crimson = 0xDC143C,
    /// <summary>
    /// Cyan: pure cyan (#00FFFF).
    /// </summary>
    Cyan = 0x00FFFF,
    /// <summary>
    /// DarkBlue: a deep blue (#00008B).
    /// </summary>
    DarkBlue = 0x00008B,
    /// <summary>
    /// DarkCyan: a deep teal (#008B8B).
    /// </summary>
    DarkCyan = 0x008B8B,
    /// <summary>
    /// DarkGoldenRod: a dark, muted gold (#B8860B).
    /// </summary>
    DarkGoldenRod = 0xB8860B,
    /// <summary>
    /// DarkGray: a medium dark gray (#A9A9A9).
    /// </summary>
    DarkGray = 0xA9A9A9,
    /// <summary>
    /// DarkGreen: a deep forest green (#006400).
    /// </summary>
    DarkGreen = 0x006400,
    /// <summary>
    /// DarkKhaki: an earthy, muted yellow (#BDB76B).
    /// </summary>
    DarkKhaki = 0xBDB76B,
    /// <summary>
    /// DarkMagenta: a dark purple-pink (#8B008B).
    /// </summary>
    DarkMagenta = 0x8B008B,
    /// <summary>
    /// DarkOliveGreen: a deep olive (#556B2F).
    /// </summary>
    DarkOliveGreen = 0x556B2F,
    /// <summary>
    /// DarkOrange: a strong burnt orange (#FF8C00).
    /// </summary>
    DarkOrange = 0xFF8C00,
    /// <summary>
    /// DarkOrchid: a rich purple (#9932CC).
    /// </summary>
    DarkOrchid = 0x9932CC,
    /// <summary>
    /// DarkRed: a deep crimson (#8B0000).
    /// </summary>
    DarkRed = 0x8B0000,
    /// <summary>
    /// DarkSalmon: a muted pinkish-orange (#E9967A).
    /// </summary>
    DarkSalmon = 0xE9967A,
    /// <summary>
    /// DarkSeaGreen: a soft, gray-green (#8FBC8F).
    /// </summary>
    DarkSeaGreen = 0x8FBC8F,
    /// <summary>
    /// DarkSlateBlue: a deep slate-purple (#483D8B).
    /// </summary>
    DarkSlateBlue = 0x483D8B,
    /// <summary>
    /// DarkSlateGray: a dark gray-green (#2F4F4F).
    /// </summary>
    DarkSlateGray = 0x2F4F4F,
    /// <summary>
    /// DarkTurquoise: a bright teal (#00CED1).
    /// </summary>
    DarkTurquoise = 0x00CED1,
    /// <summary>
    /// DarkViolet: a deep violet (#9400D3).
    /// </summary>
    DarkViolet = 0x9400D3,
    /// <summary>
    /// DeepPink: a vivid pink (#FF1493).
    /// </summary>
    DeepPink = 0xFF1493,
    /// <summary>
    /// DeepSkyBlue: a bright sky-blue (#00BFFF).
    /// </summary>
    DeepSkyBlue = 0x00BFFF,
    /// <summary>
    /// DimGray: a dark gray (#696969).
    /// </summary>
    DimGray = 0x696969,
    /// <summary>
    /// DodgerBlue: a vibrant medium blue (#1E90FF).
    /// </summary>
    DodgerBlue = 0x1E90FF,
    /// <summary>
    /// FireBrick: a strong brick-red (#B22222).
    /// </summary>
    FireBrick = 0xB22222,
    /// <summary>
    /// FloralWhite: a soft off-white with a hint of pink (#FFFAF0).
    /// </summary>
    FloralWhite = 0xFFFAF0,
    /// <summary>
    /// ForestGreen: a deep pine green (#228B22).
    /// </summary>
    ForestGreen = 0x228B22,
    /// <summary>
    /// Fuchsia: pure magenta (#FF00FF).
    /// </summary>
    Fuchsia = 0xFF00FF,
    /// <summary>
    /// Gainsboro: a very light gray (#DCDCDC).
    /// </summary>
    Gainsboro = 0xDCDCDC,
    /// <summary>
    /// GhostWhite: a very pale gray-blue (#F8F8FF).
    /// </summary>
    GhostWhite = 0xF8F8FF,
    /// <summary>
    /// Gold: a bright metallic gold (#FFD700).
    /// </summary>
    Gold = 0xFFD700,
    /// <summary>
    /// GoldenRod: a muted gold (#DAA520).
    /// </summary>
    GoldenRod = 0xDAA520,
    /// <summary>
    /// Gray: a medium gray (#808080).
    /// </summary>
    Gray = 0x808080,
    /// <summary>
    /// Green: pure green (#008000).
    /// </summary>
    Green = 0x008000,
    /// <summary>
    /// GreenYellow: a bright yellow-green (#ADFF2F).
    /// </summary>
    GreenYellow = 0xADFF2F,
    /// <summary>
    /// HoneyDew: a pale mint-green (#F0FFF0).
    /// </summary>
    HoneyDew = 0xF0FFF0,
    /// <summary>
    /// HotPink: a vivid pink (#FF69B4).
    /// </summary>
    HotPink = 0xFF69B4,
    /// <summary>
    /// IndianRed: a muted, earthy red (#CD5C5C).
    /// </summary>
    IndianRed = 0xCD5C5C,
    /// <summary>
    /// Indigo: a deep blue-purple (#4B0082).
    /// </summary>
    Indigo = 0x4B0082,
    /// <summary>
    /// Ivory: a creamy off-white (#FFFFF0).
    /// </summary>
    Ivory = 0xFFFFF0,
    /// <summary>
    /// Khaki: a warm sandy beige (#F0E68C).
    /// </summary>
    Khaki = 0xF0E68C,
    /// <summary>
    /// Lavender: a pale purple (#E6E6FA).
    /// </summary>
    Lavender = 0xE6E6FA,
    /// <summary>
    /// LavenderBlush: an off-white with a hint of pink (#FFF0F5).
    /// </summary>
    LavenderBlush = 0xFFF0F5,
    /// <summary>
    /// LawnGreen: a bright, vivid green (#7CFC00).
    /// </summary>
    LawnGreen = 0x7CFC00,
    /// <summary>
    /// LemonChiffon: a pale yellow with a hint of cream (#FFFACD).
    /// </summary>
    LemonChiffon = 0xFFFACD,
    /// <summary>
    /// LightBlue: a soft, pale blue (#ADD8E6).
    /// </summary>
    LightBlue = 0xADD8E6,
    /// <summary>
    /// LightCoral: a gentle pinkish-red (#F08080).
    /// </summary>
    LightCoral = 0xF08080,
    /// <summary>
    /// LightCyan: a very pale cyan (#E0FFFF).
    /// </summary>
    LightCyan = 0xE0FFFF,
    /// <summary>
    /// LightGoldenRodYellow: a pale, warm yellow (#FAFAD2).
    /// </summary>
    LightGoldenRodYellow = 0xFAFAD2,
    /// <summary>
    /// LightGray: a soft light gray (#D3D3D3).
    /// </summary>
    LightGray = 0xD3D3D3,
    /// <summary>
    /// LightGreen: a soft pastel green (#90EE90).
    /// </summary>
    LightGreen = 0x90EE90,
    /// <summary>
    /// LightPink: a gentle pink (#FFB6C1).
    /// </summary>
    LightPink = 0xFFB6C1,
    /// <summary>
    /// LightSalmon: a soft peach-pink (#FFA07A).
    /// </summary>
    LightSalmon = 0xFFA07A,
    /// <summary>
    /// LightSeaGreen: a medium teal (#20B2AA).
    /// </summary>
    LightSeaGreen = 0x20B2AA,
    /// <summary>
    /// LightSkyBlue: a pale sky-blue (#87CEFA).
    /// </summary>
    LightSkyBlue = 0x87CEFA,
    /// <summary>
    /// LightSlateGray: a muted gray-blue (#778899).
    /// </summary>
    LightSlateGray = 0x778899,
    /// <summary>
    /// LightSteelBlue: a soft, muted blue (#B0C4DE).
    /// </summary>
    LightSteelBlue = 0xB0C4DE,
    /// <summary>
    /// LightYellow: a very pale yellow (#FFFFE0).
    /// </summary>
    LightYellow = 0xFFFFE0,
    /// <summary>
    /// Lime: pure lime green (#00FF00).
    /// </summary>
    Lime = 0x00FF00,
    /// <summary>
    /// LimeGreen: a medium bright green (#32CD32).
    /// </summary>
    LimeGreen = 0x32CD32,
    /// <summary>
    /// Linen: a soft off-white with a hint of beige (#FAF0E6).
    /// </summary>
    Linen = 0xFAF0E6,
    /// <summary>
    /// Magenta: pure magenta (#FF00FF).
    /// </summary>
    Magenta = 0xFF00FF,
    /// <summary>
    /// Maroon: a dark reddish-brown (#800000).
    /// </summary>
    Maroon = 0x800000,
    /// <summary>
    /// MediumAquaMarine: a muted aquamarine (#66CDAA).
    /// </summary>
    MediumAquaMarine = 0x66CDAA,
    /// <summary>
    /// MediumBlue: a moderate blue (#0000CD).
    /// </summary>
    MediumBlue = 0x0000CD,
    /// <summary>
    /// MediumOrchid: a medium purple-pink (#BA55D3).
    /// </summary>
    MediumOrchid = 0xBA55D3,
    /// <summary>
    /// MediumPurple: a moderate lavender (#9370DB).
    /// </summary>
    MediumPurple = 0x9370DB,
    /// <summary>
    /// MediumSeaGreen: a subdued green (#3CB371).
    /// </summary>
    MediumSeaGreen = 0x3CB371,
    /// <summary>
    /// MediumSlateBlue: a medium slate-purple (#7B68EE).
    /// </summary>
    MediumSlateBlue = 0x7B68EE,
    /// <summary>
    /// MediumSpringGreen: a bright spring green (#00FA9A).
    /// </summary>
    MediumSpringGreen = 0x00FA9A,
    /// <summary>
    /// MediumTurquoise: a bright turquoise (#48D1CC).
    /// </summary>
    MediumTurquoise = 0x48D1CC,
    /// <summary>
    /// MediumVioletRed: a deep pinkish-red (#C71585).
    /// </summary>
    MediumVioletRed = 0xC71585,
    /// <summary>
    /// MidnightBlue: a very dark blue (#191970).
    /// </summary>
    MidnightBlue = 0x191970,
    /// <summary>
    /// MintCream: a very pale mint (#F5FFFA).
    /// </summary>
    MintCream = 0xF5FFFA,
    /// <summary>
    /// MistyRose: a soft pinkish-white (#FFE4E1).
    /// </summary>
    MistyRose = 0xFFE4E1,
    /// <summary>
    /// Moccasin: a light peach (#FFE4B5).
    /// </summary>
    Moccasin = 0xFFE4B5,
    /// <summary>
    /// NavajoWhite: a warm cream (#FFDEAD).
    /// </summary>
    NavajoWhite = 0xFFDEAD,
    /// <summary>
    /// Navy: a very dark blue (#000080).
    /// </summary>
    Navy = 0x000080,
    /// <summary>
    /// OldLace: a soft off-white with a slight yellow tint (#FDF5E6).
    /// </summary>
    OldLace = 0xFDF5E6,
    /// <summary>
    /// Olive: a dark yellow-green (#808000).
    /// </summary>
    Olive = 0x808000,
    /// <summary>
    /// OliveDrab: a muted olive green (#6B8E23).
    /// </summary>
    OliveDrab = 0x6B8E23,
    /// <summary>
    /// Orange: a bright, vivid orange (#FFA500).
    /// </summary>
    Orange = 0xFFA500,
    /// <summary>
    /// OrangeRed: a strong reddish-orange (#FF4500).
    /// </summary>
    OrangeRed = 0xFF4500,
    /// <summary>
    /// Orchid: a soft purple-pink (#DA70D6).
    /// </summary>
    Orchid = 0xDA70D6,
    /// <summary>
    /// PaleGoldenRod: a very soft gold (#EEE8AA).
    /// </summary>
    PaleGoldenRod = 0xEEE8AA,
    /// <summary>
    /// PaleGreen: a soft pastel green (#98FB98).
    /// </summary>
    PaleGreen = 0x98FB98,
    /// <summary>
    /// PaleTurquoise: a delicate cyan (#AFEEEE).
    /// </summary>
    PaleTurquoise = 0xAFEEEE,
    /// <summary>
    /// PaleVioletRed: a muted rose (#DB7093).
    /// </summary>
    PaleVioletRed = 0xDB7093,
    /// <summary>
    /// PapayaWhip: a pale peach (#FFEFD5).
    /// </summary>
    PapayaWhip = 0xFFEFD5,
    /// <summary>
    /// PeachPuff: a soft peach-pink (#FFDAB9).
    /// </summary>
    PeachPuff = 0xFFDAB9,
    /// <summary>
    /// Peru: a warm medium brown (#CD853F).
    /// </summary>
    Peru = 0xCD853F,
    /// <summary>
    /// Pink: a light true pink (#FFC0CB).
    /// </summary>
    Pink = 0xFFC0CB,
    /// <summary>
    /// Plum: a soft purple (#DDA0DD).
    /// </summary>
    Plum = 0xDDA0DD,
    /// <summary>
    /// PowderBlue: a pale bluish-gray (#B0E0E6).
    /// </summary>
    PowderBlue = 0xB0E0E6,
    /// <summary>
    /// Purple: a deep magenta-purple (#800080).
    /// </summary>
    Purple = 0x800080,
    /// <summary>
    /// RebeccaPurple: a dusky purple (#663399).
    /// </summary>
    RebeccaPurple = 0x663399,
    /// <summary>
    /// Red: pure red (#FF0000).
    /// </summary>
    Red = 0xFF0000,
    /// <summary>
    /// RosyBrown: a muted pink-brown (#BC8F8F).
    /// </summary>
    RosyBrown = 0xBC8F8F,
    /// <summary>
    /// RoyalBlue: a bright, deep blue (#4169E1).
    /// </summary>
    RoyalBlue = 0x4169E1,
    /// <summary>
    /// SaddleBrown: a rich brown (#8B4513).
    /// </summary>
    SaddleBrown = 0x8B4513,
    /// <summary>
    /// Salmon: a soft pink (#FA8072).
    /// </summary>
    Salmon = 0xFA8072,
    /// <summary>
    /// SandyBrown: a warm light brown (#F4A460).
    /// </summary>
    SandyBrown = 0xF4A460,
    /// <summary>
    /// SeaGreen: a deep green (#2E8B57).
    /// </summary>
    SeaGreen = 0x2E8B57,
    /// <summary>
    /// SeaShell: a soft off-white (#FFF5EE).
    /// </summary>
    SeaShell = 0xFFF5EE,
    /// <summary>
    /// Sienna: a rich reddish-brown (#A0522D).
    /// </summary>
    Sienna = 0xA0522D,
    /// <summary>
    /// Silver: a light metallic gray (#C0C0C0).
    /// </summary>
    Silver = 0xC0C0C0,
    /// <summary>
    /// SkyBlue: a light sky-blue (#87CEEB).
    /// </summary>
    SkyBlue = 0x87CEEB,
    /// <summary>
    /// SlateBlue: a muted purple-blue (#6A5ACD).
    /// </summary>
    SlateBlue = 0x6A5ACD,
    /// <summary>
    /// SlateGray: a medium gray with blue tint (#708090).
    /// </summary>
    SlateGray = 0x708090,
    /// <summary>
    /// Snow: a crisp white (#FFFAFA).
    /// </summary>
    Snow = 0xFFFAFA,
    /// <summary>
    /// SpringGreen: a bright green (#00FF7F).
    /// </summary>
    SpringGreen = 0x00FF7F,
    /// <summary>
    /// SteelBlue: a cool steely blue (#4682B4).
    /// </summary>
    SteelBlue = 0x4682B4,
    /// <summary>
    /// Tan: a pale brown (#D2B48C).
    /// </summary>
    Tan = 0xD2B48C,
    /// <summary>
    /// Teal: a dark cyan (#008080).
    /// </summary>
    Teal = 0x008080,
    /// <summary>
    /// Thistle: a pale lavender-gray (#D8BFD8).
    /// </summary>
    Thistle = 0xD8BFD8,
    /// <summary>
    /// Tomato: a vibrant red-orange (#FF6347).
    /// </summary>
    Tomato = 0xFF6347,
    /// <summary>
    /// Turquoise: a bright cyan-green (#40E0D0).
    /// </summary>
    Turquoise = 0x40E0D0,
    /// <summary>
    /// Violet: a light purple (#EE82EE).
    /// </summary>
    Violet = 0xEE82EE,
    /// <summary>
    /// Wheat: a soft, warm beige (#F5DEB3).
    /// </summary>
    Wheat = 0xF5DEB3,
    /// <summary>
    /// White: pure white (#FFFFFF).
    /// </summary>
    White = 0xFFFFFF,
    /// <summary>
    /// WhiteSmoke: a very light gray (#F5F5F5).
    /// </summary>
    WhiteSmoke = 0xF5F5F5,
    /// <summary>
    /// Yellow: pure yellow (#FFFF00).
    /// </summary>
    Yellow = 0xFFFF00,
    /// <summary>
    /// YellowGreen: a soft yellow-green (#9ACD32).
    /// </summary>
    YellowGreen = 0x9ACD32,

    /// <summary>
    /// Box2DRed: a vivid red used in Box2D debug draw (#DC3132).
    /// </summary>
    Box2DRed = 0xDC3132,
    /// <summary>
    /// Box2DBlue: a bright blue used in Box2D debug draw (#30AEBF).
    /// </summary>
    Box2DBlue = 0x30AEBF,
    /// <summary>
    /// Box2DGreen: a bright green used in Box2D debug draw (#8CC924).
    /// </summary>
    Box2DGreen = 0x8CC924,
    /// <summary>
    /// Box2DYellow: a soft yellow used in Box2D debug draw (#FFEE8C).
    /// </summary>
    Box2DYellow = 0xFFEE8C
}

/// <summary>
/// Extension methods for getting the components of a HexColor.
/// </summary>
[PublicAPI]
public static class HexColorExtensions
{
    /// <summary>
    /// Gets the red component of the color.
    /// </summary>
    public static byte Red(this HexColor color) => (byte)((uint)color >> 16);
    
    /// <summary>
    /// Gets the green component of the color.
    /// </summary>
    public static byte Green(this HexColor color) => (byte)((uint)color >> 8);
    
    /// <summary>
    /// Gets the blue component of the color.
    /// </summary>
    public static byte Blue(this HexColor color) => (byte)((uint)color);
}