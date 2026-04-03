using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Rendering
{
    public static class ColorUtils
    {
        public static Color FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Hex string cannot be null or empty.", nameof(hex));

            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                // RGB format
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return new Color(r, g, b);
            }
            else if (hex.Length == 8)
            {
                // ARGB format
                byte a = Convert.ToByte(hex.Substring(0, 2), 16);
                byte r = Convert.ToByte(hex.Substring(2, 2), 16);
                byte g = Convert.ToByte(hex.Substring(4, 2), 16);
                byte b = Convert.ToByte(hex.Substring(6, 2), 16);
                return new Color(r, g, b, a);
            }
            else
            {
                throw new FormatException("Hex string must be in the format RRGGBB or AARRGGBB.");
            }
        }

        public static string ToHex(Color color, bool includeAlpha = false)
        {
            if (includeAlpha)
                return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            else
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
