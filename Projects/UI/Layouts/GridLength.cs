using System;
using System.Globalization;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>A fixed logical length, automatic content size or proportional weight.</summary>
public readonly record struct GridLength
{
    /// <summary>Creates a validated length.</summary>
    public GridLength(float value, GridUnitType unit = GridUnitType.Pixel)
    {
        if (!float.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        if (unit is not (GridUnitType.Auto or GridUnitType.Pixel or GridUnitType.Proportion))
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
    public static GridLength Proportion(float weight = 1) => new(weight, GridUnitType.Proportion);
    /// <summary>Parses Auto, pixels, * or a weighted expression such as 2*.</summary>
    public static GridLength Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        text = text.Trim();
        if (text == "Auto")
            return Auto;
        bool proportional = text.EndsWith('*');
        string number = proportional ? text[..^1] : text;
        float value = proportional && number.Length == 0 ? 1 : float.Parse(number, NumberStyles.Float, CultureInfo.InvariantCulture);
        return new GridLength(value, proportional ? GridUnitType.Proportion : GridUnitType.Pixel);
    }

    /// <inheritdoc/>
    public override string ToString() => Unit == GridUnitType.Auto ? "Auto" : Value.ToString("R", CultureInfo.InvariantCulture) + (Unit == GridUnitType.Proportion ? "*" : "");
    /// <summary>Converts a logical pixel value to a grid length.</summary>
    public static implicit operator GridLength(float pixels) => new(pixels);
}
