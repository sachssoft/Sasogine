using System;
using System.Globalization;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>A finite grid length or a proportional star weight.</summary>
public readonly record struct GridLength
{
    /// <summary>Creates a validated length.</summary>
    public GridLength(float value, GridUnitType unit = GridUnitType.Pixel)
    {
        if (!float.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        if (unit is not (GridUnitType.Auto or GridUnitType.Pixel or GridUnitType.Star))
            throw new ArgumentOutOfRangeException(nameof(unit));
        Value = value;
        Unit = unit;
    }

    /// <summary>Gets pixels or weight.</summary>
    public float Value { get; }
    /// <summary>Gets the sizing mode.</summary>
    public GridUnitType Unit { get; }
    /// <summary>Gets content sizing.</summary>
    public static GridLength Auto => new(0, GridUnitType.Auto);

    /// <summary>Creates proportional sizing.</summary>
    public static GridLength Star(float weight = 1) => new(weight, GridUnitType.Star);
    /// <summary>Parses Auto, pixels, * or a weighted expression such as 2*.</summary>
    public static GridLength Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        text = text.Trim();
        if (text == "Auto")
            return Auto;
        bool star = text.EndsWith('*');
        string number = star ? text[..^1] : text;
        float value = star && number.Length == 0 ? 1 : float.Parse(number, NumberStyles.Float, CultureInfo.InvariantCulture);
        return new GridLength(value, star ? GridUnitType.Star : GridUnitType.Pixel);
    }

    /// <inheritdoc/>
    public override string ToString() => Unit == GridUnitType.Auto ? "Auto" : Value.ToString("R", CultureInfo.InvariantCulture) + (Unit == GridUnitType.Star ? "*" : "");
    /// <summary>Converts a logical pixel value to a grid length.</summary>
    public static implicit operator GridLength(float pixels) => new(pixels);
}
