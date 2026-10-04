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


    // Temperature

    /// <summary>
    /// Represents kelvin.
    /// </summary>
    public static readonly ValueUnit Kelvin =
        new(nameof(Kelvin), "K", ValueUnitGroups.Temperature, 1.0);

    /// <summary>
    /// Represents degrees Celsius.
    /// </summary>
    public static readonly ValueUnit Celsius =
        new(nameof(Celsius), "°C", ValueUnitGroups.Temperature, 1.0, 273.15);

    /// <summary>
    /// Represents degrees Fahrenheit.
    /// </summary>
    public static readonly ValueUnit Fahrenheit =
        new(nameof(Fahrenheit), "°F", ValueUnitGroups.Temperature, 5.0 / 9.0, 255.3722222222222);


    // Area

    /// <summary>
    /// Represents square millimeters.
    /// </summary>
    public static readonly ValueUnit SquareMillimeter =
        new(nameof(SquareMillimeter), "mm²", ValueUnitGroups.Area, 0.000001);

    /// <summary>
    /// Represents square centimeters.
    /// </summary>
    public static readonly ValueUnit SquareCentimeter =
        new(nameof(SquareCentimeter), "cm²", ValueUnitGroups.Area, 0.0001);

    /// <summary>
    /// Represents square meters.
    /// </summary>
    public static readonly ValueUnit SquareMeter =
        new(nameof(SquareMeter), "m²", ValueUnitGroups.Area, 1.0);

    /// <summary>
    /// Represents hectares.
    /// </summary>
    public static readonly ValueUnit Hectare =
        new(nameof(Hectare), "ha", ValueUnitGroups.Area, 10_000.0);

    /// <summary>
    /// Represents square kilometers.
    /// </summary>
    public static readonly ValueUnit SquareKilometer =
        new(nameof(SquareKilometer), "km²", ValueUnitGroups.Area, 1_000_000.0);


    // Volume

    /// <summary>
    /// Represents milliliters.
    /// </summary>
    public static readonly ValueUnit Milliliter =
        new(nameof(Milliliter), "ml", ValueUnitGroups.Volume, 0.000001);

    /// <summary>
    /// Represents liters.
    /// </summary>
    public static readonly ValueUnit Liter =
        new(nameof(Liter), "l", ValueUnitGroups.Volume, 0.001);

    /// <summary>
    /// Represents cubic centimeters.
    /// </summary>
    public static readonly ValueUnit CubicCentimeter =
        new(nameof(CubicCentimeter), "cm³", ValueUnitGroups.Volume, 0.000001);

    /// <summary>
    /// Represents cubic meters.
    /// </summary>
    public static readonly ValueUnit CubicMeter =
        new(nameof(CubicMeter), "m³", ValueUnitGroups.Volume, 1.0);


    // Data Size

    /// <summary>
    /// Represents bytes.
    /// </summary>
    public static readonly ValueUnit Byte =
        new(nameof(Byte), "B", ValueUnitGroups.DataSize, 1.0);

    /// <summary>
    /// Represents decimal kilobytes.
    /// </summary>
    public static readonly ValueUnit Kilobyte =
        new(nameof(Kilobyte), "kB", ValueUnitGroups.DataSize, 1_000.0);

    /// <summary>
    /// Represents decimal megabytes.
    /// </summary>
    public static readonly ValueUnit Megabyte =
        new(nameof(Megabyte), "MB", ValueUnitGroups.DataSize, 1_000_000.0);

    /// <summary>
    /// Represents decimal gigabytes.
    /// </summary>
    public static readonly ValueUnit Gigabyte =
        new(nameof(Gigabyte), "GB", ValueUnitGroups.DataSize, 1_000_000_000.0);

    // Angular Speed

    /// <summary>
    /// Represents radians per second.
    /// </summary>
    public static readonly ValueUnit RadianPerSecond =
        new(
            nameof(RadianPerSecond),
            "rad/s",
            ValueUnitGroups.AngularSpeed,
            1.0);

    /// <summary>
    /// Represents degrees per second.
    /// </summary>
    public static readonly ValueUnit DegreePerSecond =
        new(
            nameof(DegreePerSecond),
            "°/s",
            ValueUnitGroups.AngularSpeed,
            Math.PI / 180.0);

    /// <summary>
    /// Represents revolutions per minute.
    /// </summary>
    public static readonly ValueUnit RevolutionPerMinute =
        new(
            nameof(RevolutionPerMinute),
            "rpm",
            ValueUnitGroups.AngularSpeed,
            2.0 * Math.PI / 60.0);
}