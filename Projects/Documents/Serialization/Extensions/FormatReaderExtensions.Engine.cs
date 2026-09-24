using Sachssoft.Engine;
using Sachssoft.Engine.Gameplay;
using Sachssoft.Engine.Geometry;
using System;

namespace Sachssoft.Documents.Serialization;

/// <summary>
/// Provides markup serialization extensions for reading Engine values.
/// </summary>
public static partial class FormatReaderExtensions
{
    /// <summary>
    /// Reads a Point2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Point2 ReadPoint2(this FormatReaderBase reader, string property, Point2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var x = childReader.ReadSingle(nameof(Point2.X), fallback.X);
        var y = childReader.ReadSingle(nameof(Point2.Y), fallback.Y);

        return (new Point2(x, y));
    }


    /// <summary>
    /// Reads a Point3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Point3 ReadPoint3(this FormatReaderBase reader, string property, Point3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var x = childReader.ReadSingle(nameof(Point3.X), fallback.X);
        var y = childReader.ReadSingle(nameof(Point3.Y), fallback.Y);
        var z = childReader.ReadSingle(nameof(Point3.Z), fallback.Z);

        return (new Point3(x, y, z));
    }


    /// <summary>
    /// Reads a PixelPoint2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelPoint2 ReadPixelPoint2(this FormatReaderBase reader, string property, PixelPoint2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var x = childReader.ReadInt32(nameof(PixelPoint2.X), fallback.X);
        var y = childReader.ReadInt32(nameof(PixelPoint2.Y), fallback.Y);

        return (new PixelPoint2(x, y));
    }


    /// <summary>
    /// Reads a PixelPoint3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelPoint3 ReadPixelPoint3(this FormatReaderBase reader, string property, PixelPoint3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var x = childReader.ReadInt32(nameof(PixelPoint3.X), fallback.X);
        var y = childReader.ReadInt32(nameof(PixelPoint3.Y), fallback.Y);
        var depth = childReader.ReadInt32(nameof(PixelPoint3.Z), fallback.Z);

        return (new PixelPoint3(x, y, depth));
    }


    /// <summary>
    /// Reads a Size2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Size2 ReadSize2(this FormatReaderBase reader, string property, Size2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var width = childReader.ReadSingle(nameof(Size2.Width), fallback.Width);
        var height = childReader.ReadSingle(nameof(Size2.Height), fallback.Height);

        return (new Size2(width, height));
    }


    /// <summary>
    /// Reads a Size3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Size3 ReadSize3(this FormatReaderBase reader, string property, Size3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var width = childReader.ReadSingle(nameof(Size3.Width), fallback.Width);
        var height = childReader.ReadSingle(nameof(Size3.Height), fallback.Height);
        var depth = childReader.ReadSingle(nameof(Size3.Depth), fallback.Depth);

        return (new Size3(width, height, depth));
    }


    /// <summary>
    /// Reads a PixelSize2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelSize2 ReadPixelSize2(this FormatReaderBase reader, string property, PixelSize2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var width = childReader.ReadInt32(nameof(PixelSize2.Width), fallback.Width);
        var height = childReader.ReadInt32(nameof(PixelSize2.Height), fallback.Height);

        return (new PixelSize2(width, height));
    }


    /// <summary>
    /// Reads a PixelSize3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelSize3 ReadPixelSize3(this FormatReaderBase reader, string property, PixelSize3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var width = childReader.ReadInt32(nameof(PixelSize3.Width), fallback.Width);
        var height = childReader.ReadInt32(nameof(PixelSize3.Height), fallback.Height);
        var depth = childReader.ReadInt32(nameof(PixelSize3.Depth), fallback.Depth);

        return (new PixelSize3(width, height, depth));
    }


    /// <summary>
    /// Reads a Insets2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Insets2 ReadInsets2(this FormatReaderBase reader, string property, Insets2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var left = childReader.ReadSingle(nameof(Insets2.Left), fallback.Left);
        var top = childReader.ReadSingle(nameof(Insets2.Top), fallback.Top);
        var right = childReader.ReadSingle(nameof(Insets2.Right), fallback.Right);
        var bottom = childReader.ReadSingle(nameof(Insets2.Bottom), fallback.Bottom);

        return new Insets2(left, top, right, bottom);
    }


    /// <summary>
    /// Reads a Insets3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Insets3 ReadInsets3(this FormatReaderBase reader, string property, Insets3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var left = childReader.ReadSingle(nameof(Insets3.Left), fallback.Left);
        var top = childReader.ReadSingle(nameof(Insets3.Top), fallback.Top);
        var front = childReader.ReadSingle(nameof(Insets3.Front), fallback.Front);
        var right = childReader.ReadSingle(nameof(Insets3.Right), fallback.Right);
        var bottom = childReader.ReadSingle(nameof(Insets3.Bottom), fallback.Bottom);
        var back = childReader.ReadSingle(nameof(Insets3.Back), fallback.Back);

        return new Insets3(left, top, front, right, bottom, back);
    }


    /// <summary>
    /// Reads a PixelInsets2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelInsets2 ReadPixelInsets2(this FormatReaderBase reader, string property, PixelInsets2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var left = childReader.ReadInt32(nameof(PixelInsets2.Left), fallback.Left);
        var top = childReader.ReadInt32(nameof(PixelInsets2.Top), fallback.Top);
        var right = childReader.ReadInt32(nameof(PixelInsets2.Right), fallback.Right);
        var bottom = childReader.ReadInt32(nameof(PixelInsets2.Bottom), fallback.Bottom);

        return new PixelInsets2(left, top, right, bottom);
    }


    /// <summary>
    /// Reads a PixelInsets3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelInsets3 ReadPixelInsets3(this FormatReaderBase reader, string property, PixelInsets3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var left = childReader.ReadInt32(nameof(PixelInsets3.Left), fallback.Left);
        var top = childReader.ReadInt32(nameof(PixelInsets3.Top), fallback.Top);
        var front = childReader.ReadInt32(nameof(PixelInsets3.Front), fallback.Front);
        var right = childReader.ReadInt32(nameof(PixelInsets3.Right), fallback.Right);
        var bottom = childReader.ReadInt32(nameof(PixelInsets3.Bottom), fallback.Bottom);
        var back = childReader.ReadInt32(nameof(PixelInsets3.Back), fallback.Back);

        return new PixelInsets3(left, top, front, right, bottom, back);
    }


    /// <summary>
    /// Reads a Bounds2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Bounds2 ReadBounds2(this FormatReaderBase reader, string property, Bounds2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var x = childReader.ReadSingle(nameof(Bounds2.X), fallback.X);
        var y = childReader.ReadSingle(nameof(Bounds2.Y), fallback.Y);
        var width = childReader.ReadSingle(nameof(Bounds2.Width), fallback.Width);
        var height = childReader.ReadSingle(nameof(Bounds2.Height), fallback.Height);

        return (new Bounds2(x, y, width, height));
    }


    /// <summary>
    /// Reads a Bounds3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Bounds3 ReadBounds3(this FormatReaderBase reader, string property, Bounds3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var x = childReader.ReadSingle(nameof(Bounds3.X), fallback.X);
        var y = childReader.ReadSingle(nameof(Bounds3.Y), fallback.Y);
        var z = childReader.ReadSingle(nameof(Bounds3.Z), fallback.Z);
        var width = childReader.ReadSingle(nameof(Bounds3.Width), fallback.Width);
        var height = childReader.ReadSingle(nameof(Bounds3.Height), fallback.Height);
        var depth = childReader.ReadSingle(nameof(Bounds3.Depth), fallback.Depth);

        return (new Bounds3(x, y, z, width, height, depth));
    }


    /// <summary>
    /// Reads a PixelBounds2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelBounds2 ReadPixelBounds2(this FormatReaderBase reader, string property, PixelBounds2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var x = childReader.ReadInt32(nameof(PixelBounds2.X), fallback.X);
        var y = childReader.ReadInt32(nameof(PixelBounds2.Y), fallback.Y);
        var width = childReader.ReadInt32(nameof(PixelBounds2.Width), fallback.Width);
        var height = childReader.ReadInt32(nameof(PixelBounds2.Height), fallback.Height);

        return (new PixelBounds2(x, y, width, height));
    }


    /// <summary>
    /// Reads a PixelBounds3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelBounds3 ReadPixelBounds3(this FormatReaderBase reader, string property, PixelBounds3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var x = childReader.ReadInt32(nameof(PixelBounds3.X), fallback.X);
        var y = childReader.ReadInt32(nameof(PixelBounds3.Y), fallback.Y);
        var z = childReader.ReadInt32(nameof(PixelBounds3.Z), fallback.Z);
        var width = childReader.ReadInt32(nameof(PixelBounds3.Width), fallback.Width);
        var height = childReader.ReadInt32(nameof(PixelBounds3.Height), fallback.Height);
        var depth = childReader.ReadInt32(nameof(PixelBounds3.Depth), fallback.Depth);

        return (new PixelBounds3(x, y, z, width, height, depth));
    }


    /// <summary>
    /// Reads a Box2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Box2 ReadBox2(this FormatReaderBase reader, string property, Box2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var minX = childReader.ReadSingle(nameof(Box2.MinX), fallback.MinX);
        var minY = childReader.ReadSingle(nameof(Box2.MinY), fallback.MinY);
        var maxX = childReader.ReadSingle(nameof(Box2.MaxX), fallback.MaxX);
        var maxY = childReader.ReadSingle(nameof(Box2.MaxY), fallback.MaxY);

        return (new Box2(minX, minY, maxX, maxY));
    }


    /// <summary>
    /// Reads a Box3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Box3 ReadBox3(this FormatReaderBase reader, string property, Box3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var minX = childReader.ReadSingle(nameof(Box3.MinX), fallback.MinX);
        var minY = childReader.ReadSingle(nameof(Box3.MinY), fallback.MinY);
        var minZ = childReader.ReadSingle(nameof(Box3.MinZ), fallback.MinZ);
        var maxX = childReader.ReadSingle(nameof(Box3.MaxX), fallback.MaxX);
        var maxY = childReader.ReadSingle(nameof(Box3.MaxY), fallback.MaxY);
        var maxZ = childReader.ReadSingle(nameof(Box3.MaxZ), fallback.MaxZ);

        return (new Box3(minX, minY, minZ, maxX, maxY, maxZ));
    }


    /// <summary>
    /// Reads a PixelBox2 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelBox2 ReadPixelBox2(this FormatReaderBase reader, string property, PixelBox2 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var minX = childReader.ReadInt32(nameof(PixelBox2.MinX), fallback.MinX);
        var minY = childReader.ReadInt32(nameof(PixelBox2.MinY), fallback.MinY);
        var maxX = childReader.ReadInt32(nameof(PixelBox2.MaxX), fallback.MaxX);
        var maxY = childReader.ReadInt32(nameof(PixelBox2.MaxY), fallback.MaxY);

        return (new PixelBox2(minX, minY, maxX, maxY));
    }


    /// <summary>
    /// Reads a PixelBox3 value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static PixelBox3 ReadPixelBox3(this FormatReaderBase reader, string property, PixelBox3 fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var minX = childReader.ReadInt32(nameof(PixelBox3.MinX), fallback.MinX);
        var minY = childReader.ReadInt32(nameof(PixelBox3.MinY), fallback.MinY);
        var minZ = childReader.ReadInt32(nameof(PixelBox3.MinZ), fallback.MinZ);
        var maxX = childReader.ReadInt32(nameof(PixelBox3.MaxX), fallback.MaxX);
        var maxY = childReader.ReadInt32(nameof(PixelBox3.MaxY), fallback.MaxY);
        var maxZ = childReader.ReadInt32(nameof(PixelBox3.MaxZ), fallback.MaxZ);

        return (new PixelBox3(minX, minY, minZ, maxX, maxY, maxZ));
    }


    /// <summary>
    /// Reads a LowTieredScore value from the specified markup property.
    /// </summary>
    /// <typeparam name="TValue">The generic TValue type.</typeparam>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="readValueItem">The callback used to read an individual score value.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static LowTieredScore<TValue> ReadLowTieredScore<TValue>(
        this FormatReaderBase reader,
        string property,
        Func<FormatReaderBase, string, TValue> readValueItem,
        LowTieredScore<TValue> fallback = default
    )
         where TValue : struct, IComparable<TValue>
    {
        if (!reader.Contains(property))
            return fallback;

        var readerChild = reader.Read(property);

        if (readerChild == null)
            return fallback;

        var bronze = readValueItem(readerChild, nameof(LowTieredScore<TValue>.Bronze));
        var silver = readValueItem(readerChild, nameof(LowTieredScore<TValue>.Silver));
        var gold = readValueItem(readerChild, nameof(LowTieredScore<TValue>.Gold));

        return new LowTieredScore<TValue>(
            bronze: bronze,
            silver: silver,
            gold: gold
        );
    }


    /// <summary>
    /// Reads a HighTieredScore value from the specified markup property.
    /// </summary>
    /// <typeparam name="TValue">The generic TValue type.</typeparam>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="readValueItem">The callback used to read an individual score value.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static HighTieredScore<TValue> ReadHighTieredScore<TValue>(
        this FormatReaderBase reader,
        string property,
        Func<FormatReaderBase, string, TValue> readValueItem,
        HighTieredScore<TValue> fallback = default
    )
         where TValue : struct, IComparable<TValue>
    {
        if (!reader.Contains(property))
            return fallback;

        var readerChild = reader.Read(property);

        if (readerChild == null)
            return fallback;

        var bronze = readValueItem(readerChild, nameof(HighTieredScore<TValue>.Bronze));
        var silver = readValueItem(readerChild, nameof(HighTieredScore<TValue>.Silver));
        var gold = readValueItem(readerChild, nameof(HighTieredScore<TValue>.Gold));

        return new HighTieredScore<TValue>(
            bronze: bronze,
            silver: silver,
            gold: gold
        );
    }


    /// <summary>
    /// Reads a Segment value from the specified markup property.
    /// </summary>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">The value returned when the property cannot be read.</param>
    /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
    public static Segment ReadSegment(this FormatReaderBase reader, string property, Segment fallback)
    {
        var childReader = reader.Read(property);

        if (childReader == null)
            return fallback;

        var x1 = childReader.ReadSingle(nameof(Segment.X1), fallback.X1);
        var y1 = childReader.ReadSingle(nameof(Segment.Y1), fallback.Y1);
        var x2 = childReader.ReadSingle(nameof(Segment.X2), fallback.X2);
        var y2 = childReader.ReadSingle(nameof(Segment.Y2), fallback.Y2);

        return (new Segment(x1, y1, x2, y2));
    }
}
