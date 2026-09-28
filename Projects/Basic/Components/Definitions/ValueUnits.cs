using System;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides predefined value units.
/// </summary>
public static class ValueUnits
{
    // Screen Length

    /// <summary>
    /// Represents pixels.
    /// </summary>
    public static readonly ValueUnit Pixel =
        new(nameof(Pixel), "px", ValueUnitGroups.Length, 1.0);


    // World Length

    /// <summary>
    /// Represents millimeters.
    /// </summary>
    public static readonly ValueUnit Millimeter =
        new(nameof(Millimeter), "mm", ValueUnitGroups.Length, 0.001);

    /// <summary>
    /// Represents centimeters.
    /// </summary>
    public static readonly ValueUnit Centimeter =
        new(nameof(Centimeter), "cm", ValueUnitGroups.Length, 0.01);

    /// <summary>
    /// Represents meters.
    /// </summary>
    public static readonly ValueUnit Meter =
        new(nameof(Meter), "m", ValueUnitGroups.Length, 1.0);

    /// <summary>
    /// Represents kilometers.
    /// </summary>
    public static readonly ValueUnit Kilometer =
        new(nameof(Kilometer), "km", ValueUnitGroups.Length, 1000.0);


    // Angle

    /// <summary>
    /// Represents radians.
    /// </summary>
    public static readonly ValueUnit Radian =
        new(nameof(Radian), "rad", ValueUnitGroups.Angle, 1.0);

    /// <summary>
    /// Represents degrees.
    /// </summary>
    public static readonly ValueUnit Degree =
        new(nameof(Degree), "°", ValueUnitGroups.Angle, Math.PI / 180.0);


    // Time

    /// <summary>
    /// Represents milliseconds.
    /// </summary>
    public static readonly ValueUnit Millisecond =
        new(nameof(Millisecond), "ms", ValueUnitGroups.Time, 0.001);

    /// <summary>
    /// Represents seconds.
    /// </summary>
    public static readonly ValueUnit Second =
        new(nameof(Second), "s", ValueUnitGroups.Time, 1.0);

    /// <summary>
    /// Represents minutes.
    /// </summary>
    public static readonly ValueUnit Minute =
        new(nameof(Minute), "min", ValueUnitGroups.Time, 60.0);

    /// <summary>
    /// Represents hours.
    /// </summary>
    public static readonly ValueUnit Hour =
        new(nameof(Hour), "h", ValueUnitGroups.Time, 3600.0);


    // Frequency

    /// <summary>
    /// Represents hertz.
    /// </summary>
    public static readonly ValueUnit Hertz =
        new(nameof(Hertz), "Hz", ValueUnitGroups.Frequency, 1.0);

    /// <summary>
    /// Represents kilohertz.
    /// </summary>
    public static readonly ValueUnit Kilohertz =
        new(nameof(Kilohertz), "kHz", ValueUnitGroups.Frequency, 1000.0);

    /// <summary>
    /// Represents megahertz.
    /// </summary>
    public static readonly ValueUnit Megahertz =
        new(nameof(Megahertz), "MHz", ValueUnitGroups.Frequency, 1_000_000.0);


    // Speed

    /// <summary>
    /// Represents meters per second.
    /// </summary>
    public static readonly ValueUnit MeterPerSecond =
        new(nameof(MeterPerSecond), "m/s", ValueUnitGroups.Speed, 1.0);

    /// <summary>
    /// Represents kilometers per hour.
    /// </summary>
    public static readonly ValueUnit KilometerPerHour =
        new(nameof(KilometerPerHour), "km/h", ValueUnitGroups.Speed, 1.0 / 3.6);


    // Acceleration

    /// <summary>
    /// Represents meters per second squared.
    /// </summary>
    public static readonly ValueUnit MeterPerSecondSquared =
        new(
            nameof(MeterPerSecondSquared),
            "m/s²",
            ValueUnitGroups.Acceleration,
            1.0);


    // Mass

    /// <summary>
    /// Represents grams.
    /// </summary>
    public static readonly ValueUnit Gram =
        new(nameof(Gram), "g", ValueUnitGroups.Mass, 0.001);

    /// <summary>
    /// Represents kilograms.
    /// </summary>
    public static readonly ValueUnit Kilogram =
        new(nameof(Kilogram), "kg", ValueUnitGroups.Mass, 1.0);

    /// <summary>
    /// Represents metric tonnes.
    /// </summary>
    public static readonly ValueUnit Tonne =
        new(nameof(Tonne), "t", ValueUnitGroups.Mass, 1000.0);


    // Percentage

    /// <summary>
    /// Represents a normalized scalar value.
    /// </summary>
    public static readonly ValueUnit Ratio =
        new(nameof(Ratio), "", ValueUnitGroups.Percentage, 1.0);

    /// <summary>
    /// Represents percent.
    /// </summary>
    public static readonly ValueUnit Percent =
        new(nameof(Percent), "%", ValueUnitGroups.Percentage, 0.01);
}