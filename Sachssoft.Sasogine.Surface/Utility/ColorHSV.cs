using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Surface.Utility;

internal struct ColorHSV
{
    public int H { get; set; }
    public int S { get; set; }
    public int V { get; set; }

    public static bool operator ==(ColorHSV a, ColorHSV b)
    {
        return Equals(a, b);
    }

    public static bool operator !=(ColorHSV a, ColorHSV b)
    {
        return !Equals(a, b);
    }

    public override bool Equals(object obj)
    {
        return obj is ColorHSV && Equals((ColorHSV)obj, this);
    }

    private static bool Equals(ColorHSV a, ColorHSV b)
    {
        return a.H == b.H && a.V == b.V && a.S == b.V;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = H.GetHashCode();
            hashCode = (hashCode * 397) ^ S.GetHashCode();
            hashCode = (hashCode * 397) ^ V.GetHashCode();
            return hashCode;
        }
    }


    /// <summary>
    /// Converts RGB to HSV color system
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns>hue(0-360), saturation(0-100) and value(0-100)</returns>
    public static ColorHSV FromRGB(Color color)
    {
        var r = color.R / 255.0f;
        var g = color.G / 255.0f;
        var b = color.B / 255.0f;

        float h, s, v;
        float min, max, delta;

        min = float.Min(float.Min(r, g), b);
        max = float.Max(float.Max(r, g), b);

        v = max;

        delta = max - min;

        if (max != 0)
            s = delta / max;
        else
        {
            s = 0;
            h = 0;

            return new ColorHSV
            {
                H = (int)float.Round(h),
                S = (int)float.Round(s),
                V = (int)float.Round(v),
            };
        }

        if (delta == 0)
            h = 0;
        else
        {

            if (r == max)
                h = (g - b) / delta;
            else if (g == max)
                h = 2 + (b - r) / delta;
            else
                h = 4 + (r - g) / delta;
        }

        h *= 60;
        if (h < 0)
            h += 360;

        s *= 100;
        v *= 100;

        return new ColorHSV
        {
            H = (int)float.Round(h),
            S = (int)float.Round(s),
            V = (int)float.Round(v),
        };
    }

    /// <summary>
    /// Converts HSV color system to RGB
    /// </summary>
    /// <returns></returns>
    public Color ToRGB()
    {
        float h = H;
        if (h == 360) h = 359;
        float s = S;
        float v = V;

        int i;
        float f, p, q, t;
        h = float.Max(0.0f, float.Min(360.0f, h));
        s = float.Max(0.0f, float.Min(100.0f, s));
        v = float.Max(0.0f, float.Min(100.0f, v));
        s /= 100;
        v /= 100;
        h /= 60;
        i = (int)float.Floor(h);
        f = h - i;
        p = v * (1 - s);
        q = v * (1 - s * f);
        t = v * (1 - s * (1 - f));

        int r, g, b;
        switch (i)
        {
            case 0:
                r = (int)float.Round(255 * v);
                g = (int)float.Round(255 * t);
                b = (int)float.Round(255 * p);
                break;
            case 1:
                r = (int)float.Round(255 * q);
                g = (int)float.Round(255 * v);
                b = (int)float.Round(255 * p);
                break;
            case 2:
                r = (int)float.Round(255 * p);
                g = (int)float.Round(255 * v);
                b = (int)float.Round(255 * t);
                break;
            case 3:
                r = (int)float.Round(255 * p);
                g = (int)float.Round(255 * q);
                b = (int)float.Round(255 * v);
                break;
            case 4:
                r = (int)float.Round(255 * t);
                g = (int)float.Round(255 * p);
                b = (int)float.Round(255 * v);
                break;
            default:
                r = (int)float.Round(255 * v);
                g = (int)float.Round(255 * p);
                b = (int)float.Round(255 * q);
                break;
        }

        return new Color((byte)r, (byte)g, (byte)b, (byte)255);
    }
}