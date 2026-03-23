using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Graphics
{
    public static class ColorUtils
    {
        public static string ToHexString(this Color color, bool includeAlpha = false)
        {
            return includeAlpha
                ? $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}"
                : $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static Color Parse(string? s, Color fallback = default)
        {
            return TryParse(s, false, out var result) ? result : fallback;
        }

        public static Color Parse(string? s, bool alpha, Color fallback = default)
        {
            return TryParse(s, alpha, out var result) ? result : fallback;
        }

        public static bool TryParse(string? s, out Color result)
        {
            return TryParse(s, false, out result);
        }

        public static bool TryParse(string? s, bool alpha, out Color result)
        {
            result = default;

            if (string.IsNullOrEmpty(s))
                return false;

            // Entferne das führende "#" falls vorhanden
            if (s.StartsWith("#"))
            {
                s = s.Substring(1);
            }
            else
            {
                return false;
            }

            // Prüfen, ob die Länge des Hex-Strings korrekt ist
            int expectedLength = alpha ? 8 : 6;
            if (s.Length != expectedLength)
            {
                throw new ArgumentException("Hex string must be 6 or 8 characters in length.");
            }

            // Extrahieren der Farbkanäle aus dem Hex-String
            int r = int.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(s.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(s.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            // Wenn Alpha gewünscht ist, extrahiere auch den Alpha-Wert
            int a = alpha ? int.Parse(s.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) : 255;

            // Rückgabe der Color
            result = new Color((byte)r, (byte)g, (byte)b, (byte)a);
            return true;
        }

        public static Color ChangeAlphaChannel(Color color, float alpha)
        {
            var channel = new byte[] { color.R, color.G, color.B, color.A };
            return new Color(channel[0], channel[1], channel[2], (byte)(float.Clamp(alpha, 0f, 1f) * 255));
        }

        public static Color ChangeRedChannel(Color color, float red)
        {
            var channel = new byte[] { color.R, color.G, color.B, color.A };
            return new Color((byte)(float.Clamp(red, 0f, 1f) * 255), channel[1], channel[2], channel[3]);
        }

        public static Color ChangeGreenChannel(Color color, float green)
        {
            var channel = new byte[] { color.R, color.G, color.B, color.A };
            return new Color(channel[0], (byte)(float.Clamp(green, 0f, 1f) * 255), channel[2], channel[3]);
        }

        public static Color ChangeBlueChannel(Color color, float blue)
        {
            var channel = new byte[] { color.R, color.G, color.B, color.A };
            return new Color(channel[0], channel[1], (byte)(float.Clamp(blue, 0f, 1f) * 255), channel[3]);
        }
    }
}
