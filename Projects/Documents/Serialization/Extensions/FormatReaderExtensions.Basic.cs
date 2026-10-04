using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Geometry;
using Sachssoft.Engine.Graphics;
using System.Collections.Generic;

namespace Sachssoft.Documents.Serialization;

/// <summary>
/// Provides markup serialization extensions for reading Basic values.
/// </summary>
public static partial class FormatReaderExtensions
{
    /// <summary>
    /// Reads a <see cref="Path"/> from the specified property.
    /// </summary>
    /// <param name="reader">The reader used to deserialize the path.</param>
    /// <param name="property">The name of the property to read.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized path, or the supplied fallback when no usable value is available.</returns>
    public static Path ReadPath(this FormatReaderBase reader, string property, Path? fallback = null)
    {
        var childReader = reader.Read(property);

        if (childReader == null || !childReader.Contains(property))
            return fallback ?? Path.Empty;

        var polygons = new List<Point2[]>();
        var polygonReaders = childReader.ReadArray(property);

        foreach (var polygonReader in polygonReaders)
        {
            if (!polygonReader.Contains("Polygon"))
                continue;

            var points = new List<Point2>();
            var pointReaders = polygonReader.ReadArray("Polygon");

            foreach (var pointReader in pointReaders)
            {
                if (!pointReader.Contains("Point"))
                    continue;

                points.Add(pointReader.ReadPoint2("Point", Point2.Zero));
            }

            polygons.Add(points.ToArray());
        }

        return new Path(polygons);
    }

    /// <summary>
    /// Reads a Color value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="alpha">A value indicating whether the alpha channel is included.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Color ReadColor(this FormatReaderBase reader, string property, bool alpha = true, Color fallback = default)
    {
        var value = reader.ReadString(property, fallback.ToString());
        return ColorUtils.TryParse(value, alpha, out var result) ? result : fallback;
    }


    /// <summary>
    /// Reads a Vector2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Vector2 ReadVector2(this FormatReaderBase reader, string property, Vector2 fallback = default)
    {
        var rectReader = reader.Read(property);

        if (rectReader == null)
            return fallback;

        var x = rectReader.ReadSingle(nameof(Vector2.X), fallback.X);
        var y = rectReader.ReadSingle(nameof(Vector2.Y), fallback.Y);
        return new Vector2(x, y);
    }


    /// <summary>
    /// Reads a Vector3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Vector3 ReadVector3(this FormatReaderBase reader, string property, Vector3 fallback = default)
    {
        var rectReader = reader.Read(property);

        if (rectReader == null)
            return fallback;

        var x = rectReader.ReadSingle(nameof(Vector3.X), fallback.X);
        var y = rectReader.ReadSingle(nameof(Vector3.Y), fallback.Y);
        var z = rectReader.ReadSingle(nameof(Vector3.Z), fallback.Z);
        return new Vector3(x, y, z);
    }


    /// <summary>
    /// Reads a Vector4 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Vector4 ReadVector4(this FormatReaderBase reader, string property, Vector4 fallback = default)
    {
        var rectReader = reader.Read(property);

        if (rectReader == null)
            return fallback;

        var x = rectReader.ReadSingle(nameof(Vector4.X), fallback.X);
        var y = rectReader.ReadSingle(nameof(Vector4.Y), fallback.Y);
        var z = rectReader.ReadSingle(nameof(Vector4.Z), fallback.Z);
        var w = rectReader.ReadSingle(nameof(Vector4.W), fallback.W);
        return new Vector4(x, y, z, w);
    }
}
