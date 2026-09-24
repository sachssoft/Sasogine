using Sachssoft.Engine;
using Sachssoft.Engine.Gameplay;
using Sachssoft.Engine.Geometry;
using System;

namespace Sachssoft.Documents.Serialization;

/// <summary>
/// Provides markup serialization extensions for reading Engine values.
/// </summary>
public static partial class FormatWriterExtensions
{

    /// <summary>
    /// Writes a Point2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePoint2(this FormatWriterBase writer, string property, Point2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Point2.X), value.X);
        childWriter.WriteSingle(nameof(Point2.Y), value.Y);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a Point3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePoint3(this FormatWriterBase writer, string property, Point3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Point3.X), value.X);
        childWriter.WriteSingle(nameof(Point3.Y), value.Y);
        childWriter.WriteSingle(nameof(Point3.Z), value.Z);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelPoint2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelPoint2(this FormatWriterBase writer, string property, PixelPoint2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelPoint2.X), value.X);
        childWriter.WriteInt32(nameof(PixelPoint2.Y), value.Y);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelPoint3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelPoint3(this FormatWriterBase writer, string property, PixelPoint3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelPoint3.X), value.X);
        childWriter.WriteInt32(nameof(PixelPoint3.Y), value.Y);
        childWriter.WriteInt32(nameof(PixelPoint3.Z), value.Z);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a Size2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteSize2(this FormatWriterBase writer, string property, Size2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Size2.Width), value.Width);
        childWriter.WriteSingle(nameof(Size2.Height), value.Height);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a Size3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteSize3(this FormatWriterBase writer, string property, Size3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Size3.Width), value.Width);
        childWriter.WriteSingle(nameof(Size3.Height), value.Height);
        childWriter.WriteSingle(nameof(Size3.Depth), value.Depth);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelSize2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelSize2(this FormatWriterBase writer, string property, PixelSize2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelSize2.Width), value.Width);
        childWriter.WriteInt32(nameof(PixelSize2.Height), value.Height);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelSize3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelSize3(this FormatWriterBase writer, string property, PixelSize3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelSize3.Width), value.Width);
        childWriter.WriteInt32(nameof(PixelSize3.Height), value.Height);
        childWriter.WriteInt32(nameof(PixelSize3.Depth), value.Depth);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a Insets2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteInsets2(this FormatWriterBase writer, string property, Insets2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Insets2.Left), value.Left);
        childWriter.WriteSingle(nameof(Insets2.Top), value.Top);
        childWriter.WriteSingle(nameof(Insets2.Right), value.Right);
        childWriter.WriteSingle(nameof(Insets2.Bottom), value.Bottom);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a Insets3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteInsets3(this FormatWriterBase writer, string property, Insets3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Insets3.Left), value.Left);
        childWriter.WriteSingle(nameof(Insets3.Top), value.Top);
        childWriter.WriteSingle(nameof(Insets3.Front), value.Front);
        childWriter.WriteSingle(nameof(Insets3.Right), value.Right);
        childWriter.WriteSingle(nameof(Insets3.Bottom), value.Bottom);
        childWriter.WriteSingle(nameof(Insets3.Back), value.Back);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelInsets2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelInsets2(this FormatWriterBase writer, string property, PixelInsets2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelInsets2.Left), value.Left);
        childWriter.WriteInt32(nameof(PixelInsets2.Top), value.Top);
        childWriter.WriteInt32(nameof(PixelInsets2.Right), value.Right);
        childWriter.WriteInt32(nameof(PixelInsets2.Bottom), value.Bottom);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelInsets3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelInsets3(this FormatWriterBase writer, string property, PixelInsets3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelInsets3.Left), value.Left);
        childWriter.WriteInt32(nameof(PixelInsets3.Top), value.Top);
        childWriter.WriteInt32(nameof(PixelInsets3.Front), value.Front);
        childWriter.WriteInt32(nameof(PixelInsets3.Right), value.Right);
        childWriter.WriteInt32(nameof(PixelInsets3.Bottom), value.Bottom);
        childWriter.WriteInt32(nameof(PixelInsets3.Back), value.Back);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a Bounds2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteBounds2(this FormatWriterBase writer, string property, Bounds2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Bounds2.X), value.X);
        childWriter.WriteSingle(nameof(Bounds2.Y), value.Y);
        childWriter.WriteSingle(nameof(Bounds2.Width), value.Width);
        childWriter.WriteSingle(nameof(Bounds2.Height), value.Height);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a Bounds3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteBounds3(this FormatWriterBase writer, string property, Bounds3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Bounds3.X), value.X);
        childWriter.WriteSingle(nameof(Bounds3.Y), value.Y);
        childWriter.WriteSingle(nameof(Bounds3.Z), value.Z);
        childWriter.WriteSingle(nameof(Bounds3.Width), value.Width);
        childWriter.WriteSingle(nameof(Bounds3.Height), value.Height);
        childWriter.WriteSingle(nameof(Bounds3.Depth), value.Depth);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelBounds2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelBounds2(this FormatWriterBase writer, string property, PixelBounds2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelBounds2.X), value.X);
        childWriter.WriteInt32(nameof(PixelBounds2.Y), value.Y);
        childWriter.WriteInt32(nameof(PixelBounds2.Width), value.Width);
        childWriter.WriteInt32(nameof(PixelBounds2.Height), value.Height);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelBounds3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelBounds3(this FormatWriterBase writer, string property, PixelBounds3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelBounds3.X), value.X);
        childWriter.WriteInt32(nameof(PixelBounds3.Y), value.Y);
        childWriter.WriteInt32(nameof(PixelBounds3.Z), value.Z);
        childWriter.WriteInt32(nameof(PixelBounds3.Width), value.Width);
        childWriter.WriteInt32(nameof(PixelBounds3.Height), value.Height);
        childWriter.WriteInt32(nameof(PixelBounds3.Depth), value.Depth);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a Box2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteBox2(this FormatWriterBase writer, string property, Box2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Box2.MinX), value.MinX);
        childWriter.WriteSingle(nameof(Box2.MinY), value.MinY);
        childWriter.WriteSingle(nameof(Box2.MaxX), value.MaxX);
        childWriter.WriteSingle(nameof(Box2.MaxY), value.MaxY);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a Box3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteBox3(this FormatWriterBase writer, string property, Box3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Box3.MinX), value.MinX);
        childWriter.WriteSingle(nameof(Box3.MinY), value.MinY);
        childWriter.WriteSingle(nameof(Box3.MinZ), value.MinZ);
        childWriter.WriteSingle(nameof(Box3.MaxX), value.MaxX);
        childWriter.WriteSingle(nameof(Box3.MaxY), value.MaxY);
        childWriter.WriteSingle(nameof(Box3.MaxZ), value.MaxZ);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelBox2 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelBox2(this FormatWriterBase writer, string property, PixelBox2 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelBox2.MinX), value.MinX);
        childWriter.WriteInt32(nameof(PixelBox2.MinY), value.MinY);
        childWriter.WriteInt32(nameof(PixelBox2.MaxX), value.MaxX);
        childWriter.WriteInt32(nameof(PixelBox2.MaxY), value.MaxY);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a PixelBox3 value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WritePixelBox3(this FormatWriterBase writer, string property, PixelBox3 value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteInt32(nameof(PixelBox3.MinX), value.MinX);
        childWriter.WriteInt32(nameof(PixelBox3.MinY), value.MinY);
        childWriter.WriteInt32(nameof(PixelBox3.MinZ), value.MinZ);
        childWriter.WriteInt32(nameof(PixelBox3.MaxX), value.MaxX);
        childWriter.WriteInt32(nameof(PixelBox3.MaxY), value.MaxY);
        childWriter.WriteInt32(nameof(PixelBox3.MaxZ), value.MaxZ);

        writer.Write(property, childWriter);
    }


    /// <summary>
    /// Writes a LowTieredScore value to the specified markup property.
    /// </summary>
    /// <typeparam name="TValue">The generic TValue type.</typeparam>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="writeValueItem">The callback used to write an individual score value.</param>
    public static void WriteLowTieredScore<TValue>(
        this FormatWriterBase writer,
        string property,
        LowTieredScore<TValue> value,
        Action<FormatWriterBase, string, TValue?> writeValueItem
    )
         where TValue : struct, IComparable<TValue>
    {
        var writerChild = writer.CreateWriter();

        writeValueItem(writerChild, nameof(LowTieredScore<TValue>.Bronze), value.Bronze);
        writeValueItem(writerChild, nameof(LowTieredScore<TValue>.Silver), value.Silver);
        writeValueItem(writerChild, nameof(LowTieredScore<TValue>.Gold), value.Gold);
        writer.Write(property, writerChild);
    }


    /// <summary>
    /// Writes a HighTieredScore value to the specified markup property.
    /// </summary>
    /// <typeparam name="TValue">The generic TValue type.</typeparam>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="writeValueItem">The callback used to write an individual score value.</param>
    public static void WriteHighTieredScore<TValue>(
        this FormatWriterBase writer,
        string property,
        HighTieredScore<TValue> value,
        Action<FormatWriterBase, string, TValue?> writeValueItem
    )
         where TValue : struct, IComparable<TValue>
    {
        var writerChild = writer.CreateWriter();

        writeValueItem(writerChild, nameof(HighTieredScore<TValue>.Bronze), value.Bronze);
        writeValueItem(writerChild, nameof(HighTieredScore<TValue>.Silver), value.Silver);
        writeValueItem(writerChild, nameof(HighTieredScore<TValue>.Gold), value.Gold);
        writer.Write(property, writerChild);
    }


    /// <summary>
    /// Writes a Segment value to the specified markup property.
    /// </summary>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteSegment(this FormatWriterBase writer, string property, Segment value)
    {
        var childWriter = writer.CreateWriter();

        childWriter.WriteSingle(nameof(Segment.X1), value.X1);
        childWriter.WriteSingle(nameof(Segment.Y1), value.Y1);
        childWriter.WriteSingle(nameof(Segment.X2), value.X2);
        childWriter.WriteSingle(nameof(Segment.Y2), value.Y2);

        writer.Write(property, childWriter);
    }
}
