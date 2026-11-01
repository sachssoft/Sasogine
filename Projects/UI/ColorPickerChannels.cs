using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics.Colors;
using System;

namespace Sachssoft.Engine.Controls;

/// <summary>Maps supported color models to normalized plane coordinates without runtime discovery.</summary>
internal static class ColorPickerChannels
{
    /// <summary>Converts a color to three normalized model coordinates and straight alpha.</summary>
    /// <param name="color">The straight RGBA color.</param>
    /// <param name="model">The explicitly selected representation.</param>
    /// <returns>Coordinates in the unit cube; OKLab a and b are offset by one half.</returns>
    internal static Vector4 Read(Color color, ColorPickerModel model)
    {
        switch (model)
        {
            case ColorPickerModel.Hsl:
                var hsl = HslColor.FromColor(color);
                return new Vector4(hsl.Hue / 360f, hsl.Saturation, hsl.Lightness, hsl.Alpha);
            case ColorPickerModel.Hsv:
                var hsv = HsvColor.FromColor(color);
                return new Vector4(hsv.Hue / 360f, hsv.Saturation, hsv.Value, hsv.Alpha);
            case ColorPickerModel.LinearRgb:
                var linear = LinearRgbColor.FromColor(color);
                return new Vector4(linear.Red, linear.Green, linear.Blue, linear.Alpha);
            case ColorPickerModel.Oklab:
                var lab = OklabColor.FromColor(color);
                return new Vector4(lab.Lightness, lab.A + 0.5f, lab.B + 0.5f, lab.Alpha);
            default:
                return color.ToVector4();
        }
    }

    /// <summary>Converts normalized coordinates back to displayable straight RGBA.</summary>
    /// <param name="value">The normalized components, including alpha.</param>
    /// <param name="model">The explicitly selected representation.</param>
    /// <returns>The engine-converted color, clipped to its displayable gamut.</returns>
    internal static Color Write(Vector4 value, ColorPickerModel model)
    {
        return model switch
        {
            ColorPickerModel.Hsl => new HslColor(value.X * 360f, value.Y, value.Z, value.W).ToColor(),
            ColorPickerModel.Hsv => new HsvColor(value.X * 360f, value.Y, value.Z, value.W).ToColor(),
            ColorPickerModel.LinearRgb => new LinearRgbColor(value.X, value.Y, value.Z, value.W).ToColor(),
            ColorPickerModel.Oklab => new OklabColor(value.X, value.Y - 0.5f, value.Z - 0.5f, value.W).ToColor(),
            _ => new Color(value)
        };
    }

    /// <summary>Reads one component without boxing or temporary collections.</summary>
    /// <param name="value">The normalized coordinates.</param>
    /// <param name="index">The zero-based component index.</param>
    /// <returns>The requested coordinate.</returns>
    internal static float Get(Vector4 value, int index) => index switch
    {
        0 => value.X,
        1 => value.Y,
        2 => value.Z,
        _ => value.W
    };

    /// <summary>Changes one normalized coordinate in place.</summary>
    /// <param name="value">The coordinates to change.</param>
    /// <param name="index">The zero-based component index.</param>
    /// <param name="component">The new coordinate.</param>
    internal static void Set(ref Vector4 value, int index, float component)
    {
        switch (index)
        {
            case 0: value.X = component; break;
            case 1: value.Y = component; break;
            case 2: value.Z = component; break;
            default: value.W = component; break;
        }
    }
}
