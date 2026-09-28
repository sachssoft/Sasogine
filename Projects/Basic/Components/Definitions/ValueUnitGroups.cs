namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides predefined groups for compatible value units.
/// </summary>
/// <remarks>
/// A value unit group defines a physical or logical quantity and is used
/// to determine whether two value units can be converted between each other.
/// </remarks>
public static class ValueUnitGroups
{
    /// <summary>
    /// Represents units of length, such as meters, centimeters, or pixels.
    /// </summary>
    public static readonly ValueUnitGroup Length = new("length");

    /// <summary>
    /// Represents units of angular measurement, such as degrees or radians.
    /// </summary>
    public static readonly ValueUnitGroup Angle = new("angle");

    /// <summary>
    /// Represents units of time, such as seconds, milliseconds, or minutes.
    /// </summary>
    public static readonly ValueUnitGroup Time = new("time");

    /// <summary>
    /// Represents units of speed, such as meters per second or kilometers per hour.
    /// </summary>
    public static readonly ValueUnitGroup Speed = new("speed");

    /// <summary>
    /// Represents units of acceleration, such as meters per second squared.
    /// </summary>
    public static readonly ValueUnitGroup Acceleration = new("acceleration");

    /// <summary>
    /// Represents units of mass, such as grams or kilograms.
    /// </summary>
    public static readonly ValueUnitGroup Mass = new("mass");

    /// <summary>
    /// Represents units of temperature, such as degrees Celsius or Fahrenheit.
    /// </summary>
    public static readonly ValueUnitGroup Temperature = new("temperature");

    /// <summary>
    /// Represents relative or percentage-based values.
    /// </summary>
    public static readonly ValueUnitGroup Percentage = new("percentage");

    /// <summary>
    /// Represents units of area.
    /// </summary>
    public static readonly ValueUnitGroup Area = new("area");

    /// <summary>
    /// Represents units of volume.
    /// </summary>
    public static readonly ValueUnitGroup Volume = new("volume");

    /// <summary>
    /// Represents units of frequency.
    /// </summary>
    public static readonly ValueUnitGroup Frequency = new("frequency");

    /// <summary>
    /// Represents units of data size, such as bytes, kilobytes, or megabytes.
    /// </summary>
    public static readonly ValueUnitGroup DataSize = new("data-size");
}