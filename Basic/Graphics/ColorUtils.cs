using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace Sachssoft.Sasogine.Graphics;

public static class ColorUtils
{
    /// <summary>
    /// Erstellt eine Color aus einem Hex-String. RRGGBB oder AARRGGBB.
    /// </summary>
    public static Color FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Hex string cannot be null or empty.", nameof(hex));

        hex = hex.TrimStart('#');

        byte r, g, b, a = 255;

        if (hex.Length == 6)
        {
            r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
        }
        else if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
        }
        else
        {
            throw new FormatException("Hex string must be RRGGBB or AARRGGBB.");
        }

        return new Color(r, g, b, a);
    }

    /// <summary>
    /// Parst einen Hex-String in eine Color. Gibt fallback zurück, wenn ungültig.
    /// </summary>
    public static Color Parse(string? s, bool alpha = false, Color fallback = default)
    {
        if (TryParse(s, alpha, out var result))
            return result;

        return fallback;
    }

    /// <summary>
    /// Versucht, einen Hex-String in eine Color zu parsen.
    /// </summary>
    public static bool TryParse(string? s, bool alpha, out Color result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s) || !s.StartsWith("#"))
            return false;

        s = s.Substring(1);
        int expectedLength = alpha ? 8 : 6;
        if (s.Length != expectedLength)
            return false;

        try
        {
            byte r = byte.Parse(s.Substring(alpha ? 2 : 0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(s.Substring(alpha ? 4 : 2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(s.Substring(alpha ? 6 : 4, 2), NumberStyles.HexNumber);
            byte a = alpha ? byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber) : (byte)255;

            result = new Color(r, g, b, a);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static Color ChangeAlpha(Color color, float alpha)
    {
        return new Color(color.R, color.G, color.B, (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255));
    }

    public static Color ChangeRed(Color color, float red)
    {
        return new Color((byte)(MathHelper.Clamp(red, 0f, 1f) * 255), color.G, color.B, color.A);
    }

    public static Color ChangeGreen(Color color, float green)
    {
        return new Color(color.R, (byte)(MathHelper.Clamp(green, 0f, 1f) * 255), color.B, color.A);
    }

    public static Color ChangeBlue(Color color, float blue)
    {
        return new Color(color.R, color.G, (byte)(MathHelper.Clamp(blue, 0f, 1f) * 255), color.A);
    }
}