using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Controllers;
using Sachssoft.Inspection;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Containers;
using Sachssoft.Sasogine.Gameplay;
using Sachssoft.Sasogine.Geometry;
using Sachssoft.Sasogine.Graphics.Colors;
using Sachssoft.Sasogine.Resources.Wrappers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Formats.Tar;
using System.IO;
using System.Reflection.PortableExecutable;

namespace Sachssoft.Documents;

public static class ObjectSerializerExtensions
{
    public static Point ReadPoint<TReader>(this TReader reader, string property, Point fallback = default) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPoint(reader: reader, context: property, fallback: fallback);
    }

    public static Point ReadPoint<TReader>(this TReader reader, object? context, Point fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadInt32Array(
            context,
            new int[] {
                fallback.X,
                fallback.Y
            }
        );
        return new Point(result[0], result[1]);
    }

    public static Rectangle ReadRectangle<TReader>(this TReader reader, string property, Rectangle fallback = default) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadRectangle(reader: reader, context: property, fallback: fallback);
    }

    public static Rectangle ReadRectangle<TReader>(this TReader reader, object? context, Rectangle fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadInt32Array(
            context,
            new int[] {
                fallback.X,
                fallback.Y,
                fallback.Width,
                fallback.Height
            }
        );
        return new Rectangle(result[0], result[1], result[2], result[3]);
    }

    public static Color ReadColor<TReader>(this TReader reader, string property, Color fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadColor(reader: reader, context: property, fallback: fallback);
    }

    public static Color ReadColor<TReader>(this TReader reader, object? context, Color fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        var result = reader.ReadString(context, fallback.ToHex(alpha))!;
        return result.FromHexToColor(alpha);
    }

    public static Vector2 ReadVector2<TReader>(this TReader reader, string property, Vector2 fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadVector2(reader: reader, context: property, fallback: fallback);
    }

    public static Vector2 ReadVector2<TReader>(this TReader reader, object? context, Vector2 fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.X,
                fallback.Y
            }
        );
        return new Vector2(result[0], result[1]);
    }

    public static Vector3 ReadVector3<TReader>(this TReader reader, string property, Vector3 fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadVector3(reader: reader, context: property, fallback: fallback);
    }

    public static Vector3 ReadVector3<TReader>(this TReader reader, object? context, Vector3 fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.X,
                fallback.Y,
                fallback.Z
            }
        );
        return new Vector3(result[0], result[1], result[2]);
    }

    public static Vector4 ReadVector4<TReader>(this TReader reader, string property, Vector4 fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadVector4(reader: reader, context: property, fallback: fallback);
    }

    public static Vector4 ReadVector4<TReader>(this TReader reader, object? context, Vector4 fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.X,
                fallback.Y,
                fallback.Z,
                fallback.W
            }
        );
        return new Vector4(result[0], result[1], result[2], result[3]);
    }

    public static Quaternion ReadQuaternion<TReader>(this TReader reader, string property, Quaternion fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadQuaternion(reader: reader, context: property, fallback: fallback);
    }

    public static Quaternion ReadQuaternion<TReader>(this TReader reader, object? context, Quaternion fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.X,
                fallback.Y,
                fallback.Z,
                fallback.W
            }
        );
        return new Quaternion(result[0], result[1], result[2], result[3]);
    }

    public static Matrix ReadMatrix<TReader>(this TReader reader, string property, Matrix fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadMatrix(reader: reader, context: property, fallback: fallback);
    }

    public static Matrix ReadMatrix<TReader>(this TReader reader, object? context, Matrix fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.M11,
                fallback.M12,
                fallback.M13,
                fallback.M14,
                fallback.M21,
                fallback.M22,
                fallback.M23,
                fallback.M24,
                fallback.M31,
                fallback.M32,
                fallback.M33,
                fallback.M34,
                fallback.M41,
                fallback.M42,
                fallback.M43,
                fallback.M44
            }
        );

        return new Matrix(
            result[0], result[1], result[2], result[3],
            result[4], result[5], result[6], result[7],
            result[8], result[9], result[10], result[11],
            result[12], result[13], result[14], result[15]);
    }

    public static Plane ReadPlane<TReader>(this TReader reader, string property, Plane fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPlane(reader: reader, context: property, fallback: fallback);
    }

    public static Plane ReadPlane<TReader>(this TReader reader, object? context, Plane fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.Normal.X,
                fallback.Normal.Y,
                fallback.Normal.Z,
                fallback.D
            }
        );
        return new Plane(new Vector3(result[0], result[1], result[2]), result[3]);
    }

    public static Ray ReadRay<TReader>(this TReader reader, string property, Ray fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadRay(reader: reader, context: property, fallback: fallback);
    }

    public static Ray ReadRay<TReader>(this TReader reader, object? context, Ray fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.Position.X,
                fallback.Position.Y,
                fallback.Position.Z,
                fallback.Direction.X,
                fallback.Direction.Y,
                fallback.Direction.Z
            }
        );
        return new Ray(new Vector3(result[0], result[1], result[2]), new Vector3(result[3], result[4], result[5]));
    }

    public static BoundingBox ReadBoundingBox<TReader>(this TReader reader, string property, BoundingBox fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadBoundingBox(reader: reader, context: property, fallback: fallback);
    }

    public static BoundingBox ReadBoundingBox<TReader>(this TReader reader, object? context, BoundingBox fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.Min.X,
                fallback.Min.Y,
                fallback.Min.Z,
                fallback.Max.X,
                fallback.Max.Y,
                fallback.Max.Z
            }
        );
        return new BoundingBox(new Vector3(result[0], result[1], result[2]), new Vector3(result[3], result[4], result[5]));
    }

    public static BoundingSphere ReadBoundingSphere<TReader>(this TReader reader, string property, BoundingSphere fallback = default, bool alpha = false) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadBoundingSphere(reader: reader, context: property, fallback: fallback);
    }

    public static BoundingSphere ReadBoundingSphere<TReader>(this TReader reader, object? context, BoundingSphere fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.Center.X,
                fallback.Center.Y,
                fallback.Center.Z,
                fallback.Radius
            }
        );
        return new BoundingSphere(new Vector3(result[0], result[1], result[2]), result[3]);
    }

    public static PathCollection? ReadPathCollection<TReader>(
        this TReader reader,
        object? context,
        PathCollection? fallback = default,
        SerializationFormat format = SerializationFormat.Compact)
        where TReader : FormatReaderBase
    {
        switch (format)
        {
            case SerializationFormat.Underlying:
                {
                    //var pathNodes = reader.ReadArray(context);
                    //if (pathNodes == null)
                    //    return fallback;

                    //var paths = new PathCollection();

                    //foreach (var pathNode in pathNodes)
                    //{
                    //    var path = new Sasogine.Geometry.Path();

                    //    var polygonNodes = reader.ReadArray(pathNode);
                    //    if (polygonNodes != null)
                    //    {
                    //        foreach (var polygonNode in polygonNodes)
                    //        {
                    //            int pointCount = reader.ReadArrayCount(polygonNode);
                    //            var polygonPoints = new List<Sasogine.Geometry.Point>();

                    //            for (int i = 0; i < pointCount; i++)
                    //            {
                    //                var pointArray = reader.ReadSingleArray<float>(polygonNode, i);
                    //                if (pointArray.Length >= 2)
                    //                {
                    //                    polygonPoints.Add(new Sasogine.Geometry.Point(pointArray[0], pointArray[1]));
                    //                }
                    //            }

                    //            path.AddPolygon(polygonPoints);
                    //        }
                    //    }

                    //    paths.Add(path);
                    //}

                    //return paths;
                    throw new NotImplementedException();
                }

            case SerializationFormat.Compact:
                {
                    string? base64 = reader.ReadString(context);
                    if (string.IsNullOrEmpty(base64))
                        return fallback;

                    var bytes = Convert.FromBase64String(base64);
                    using var ms = new MemoryStream(bytes);
                    using var binaryReader = new BinaryReader(ms);

                    var paths = new List<Sasogine.Geometry.Path>();

                    try
                    {
                        int pathCount = binaryReader.ReadInt32();

                        for (int p = 0; p < pathCount; p++)
                        {
                            var polygons = new List<Vector2[]>();
                            int polygonCount = binaryReader.ReadInt32();

                            for (int i = 0; i < polygonCount; i++)
                            {
                                int pointCount = binaryReader.ReadInt32();
                                var polygonPoints = new List<Vector2>();

                                for (int j = 0; j < pointCount; j++)
                                {
                                    float x = binaryReader.ReadSingle();
                                    float y = binaryReader.ReadSingle();
                                    polygonPoints.Add(new Vector2(x, y));
                                }

                                polygons.Add(polygonPoints.ToArray());
                            }

                            paths.Add(new Sasogine.Geometry.Path(polygons.ToArray()));
                        }

                        return new PathCollection(paths);
                    }
                    catch
                    {
                        return fallback;
                    }
                }

            default:
                throw new NotSupportedException($"Serialization format {format} not supported.");
        }
    }


    public static Texture2D? ReadTexture2D<TReader>(this TReader reader, string property, GraphicsDevice graphics_device, Texture2D? fallback = default, SerializationFormat format = SerializationFormat.Compact) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadTexture2D(reader: reader, context: property, fallback: fallback, graphicsDevice: graphics_device);
    }

    public static Texture2D? ReadTexture2D<TReader>(this TReader reader, object? context, GraphicsDevice graphicsDevice, Texture2D? fallback = default, SerializationFormat format = SerializationFormat.Compact) where TReader : FormatReaderBase
    {
        var wrapper = FetchTexture2D<TReader>(reader, context, format);

        if (wrapper == null)
            return fallback;

        wrapper.GraphicsDevice = graphicsDevice;
        wrapper.Open();

        if (wrapper.Result == null)
            return fallback;
        return wrapper.Result;
    }

    public static Texture2DWrapper? FetchTexture2D<TReader>(this TReader reader, object? context, SerializationFormat format = SerializationFormat.Compact) where TReader : FormatReaderBase
    {
        byte[] texture_bytes;

        switch (format)
        {
            case SerializationFormat.Compact:
                {
                    var base64_string = reader.ReadString(context, string.Empty);
                    if (string.IsNullOrEmpty(base64_string))
                        return null;

                    texture_bytes = Convert.FromBase64String(base64_string);
                }
                break;
            case SerializationFormat.Underlying:
                {
                    texture_bytes = reader.ReadByteArray(context, null);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        if (texture_bytes == null || texture_bytes.Length == 0)
            return null;
        return new Texture2DWrapper(texture_bytes);
    }

    public static TieredScore<int> ReadTieredScoreInt<TReader>(this TReader reader, object? context, ScoreDirection scoreDirection = ScoreDirection.High, TieredScore<int> fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadInt32Array(
            context,
            new int[] {
                fallback.Gold,
                fallback.Silver,
                fallback.Bronze
            }
        );
        return new TieredScore<int>(result[0], result[1], result[2], scoreDirection);
    }

    public static TieredScore<float> ReadTieredScoreSingle<TReader>(this TReader reader, object? context, ScoreDirection scoreDirection = ScoreDirection.High, TieredScore<float> fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadSingleArray(
            context,
            new float[] {
                fallback.Gold,
                fallback.Silver,
                fallback.Bronze
            }
        );
        return new TieredScore<float>(result[0], result[1], result[2], scoreDirection);
    }

    public static TieredScore<TimeSpan> ReadTieredScoreTimeSpan<TReader>(this TReader reader, object? context, ScoreDirection scoreDirection = ScoreDirection.Low, TieredScore<TimeSpan> fallback = default) where TReader : FormatReaderBase
    {
        var result = reader.ReadTimeSpanArray(
            context,
            new TimeSpan[] {
                fallback.Gold,
                fallback.Silver,
                fallback.Bronze
            }
        );
        return new TieredScore<TimeSpan>(result[0], result[1], result[2], scoreDirection);
    }

    public static AssetReference<TAsset> ReadAssetReference<TReader, TAsset>(this TReader reader, object? context) where TReader : FormatReaderBase where TAsset : class, IIdentifiable
    {
        var reference = new AssetReference<TAsset>();
        reference.ID = reader.ReadString(context);
        return reference;
    }

    public static void WritePoint<TWriter>(this TWriter writer, object? context, Point value) where TWriter : FormatWriterBase
    {
        writer.WriteInt32Array(context, new int[] { value.X, value.Y });
    }

    public static void WritePoint<TWriter>(this TWriter writer, string property, Point value) where TWriter : FormatWriterBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        WritePoint(writer, context: property, value);
    }

    public static void WriteRectangle<TWriter>(this TWriter writer, object? context, Rectangle value) where TWriter : FormatWriterBase
    {
        writer.WriteInt32Array(context, new int[] { value.X, value.Y, value.Width, value.Height });
    }

    public static void WriteColor<TWriter>(this TWriter writer, object? context, Color? value, bool alpha = false) where TWriter : FormatWriterBase
    {
        writer.WriteString(context, value?.ToHex(alpha));
    }

    public static void WriteVector2<TWriter>(this TWriter writer, object? context, Vector2 value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context, new float[] { value.X, value.Y });
    }

    public static void WriteVector3<TWriter>(this TWriter writer, object? context, Vector3 value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context, new float[] { value.X, value.Y, value.Z });
    }

    public static void WriteVector4<TWriter>(this TWriter writer, object? context, Vector4 value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context, new float[] { value.X, value.Y, value.Z, value.W });
    }

    public static void WriteQuaternion<TWriter>(this TWriter writer, object? context, Quaternion value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context, new float[] { value.X, value.Y, value.Z, value.W });
    }

    public static void WriteMatrix<TWriter>(this TWriter writer, object? context, Matrix value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context,
            new float[] {
                value.M11, value.M12, value.M13, value.M14,
                value.M21, value.M22, value.M23, value.M24,
                value.M31, value.M32, value.M33, value.M34,
                value.M41, value.M42, value.M43, value.M44
        });
    }

    public static void WritePlane<TWriter>(this TWriter writer, object? context, Plane value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context,
            new float[] {
                value.Normal.X, value.Normal.Y, value.Normal.Z, value.D
        });
    }

    public static void WriteRay<TWriter>(this TWriter writer, object? context, Ray value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context,
            new float[] {
                value.Position.X, value.Position.Y, value.Position.Z,
                value.Direction.X, value.Direction.Y, value.Direction.Z
        });
    }

    public static void WriteBoundingBox<TWriter>(this TWriter writer, object? context, BoundingBox value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context,
            new float[] {
                value.Min.X, value.Min.Y, value.Min.Z,
                value.Max.X, value.Max.Y, value.Max.Z
        });
    }

    public static void WriteBoundingSphere<TWriter>(this TWriter writer, object? context, BoundingSphere value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context,
            new float[] {
                value.Center.X, value.Center.Y, value.Center.Z,
                value.Radius
        });
    }

    public static void WritePath<TWriter>(this TWriter writer, object? context, Sasogine.Geometry.Path? value, SerializationFormat format = SerializationFormat.Compact) where TWriter : FormatWriterBase
    {
        if (value == null)
        {
            writer.WriteString(context: context, null);
            return;
        }

        switch (format)
        {
            case SerializationFormat.Underlying:
                {
                    // Write each polygon as array of points
                    var polygonWriters = new List<TWriter>();

                    for (int i = 0; i < value.GetPolygonCount(); i++)
                    {
                        var pointWriters = new List<TWriter>();

                        for (int j = 0; j < value.GetPointCount(i); j++)
                        {
                            var point = value.GetPoint(i, j);
                            var pointWriter = (TWriter)writer.CreateWriter();
                            pointWriter.WriteSingleArray(new float[] { point.X, point.Y });
                            pointWriters.Add(pointWriter);
                        }

                        var polygonWriter = (TWriter)writer.CreateWriter();
                        polygonWriter.WriteArray(pointWriters.ToArray());
                        polygonWriters.Add(polygonWriter);
                    }

                    // Write all polygons into the main writer
                    writer.WriteArray(polygonWriters.ToArray());
                    break;
                }

            case SerializationFormat.Compact:
                {
                    // Serialize path as compact binary (Base64)
                    using var ms = new MemoryStream();
                    using var binaryWriter = new BinaryWriter(ms);

                    // Number of polygons
                    binaryWriter.Write(value.GetPolygonCount());

                    for (int i = 0; i < value.GetPolygonCount(); i++)
                    {
                        // Number of points in polygon
                        binaryWriter.Write(value.GetPointCount(i));

                        for (int j = 0; j < value.GetPointCount(i); j++)
                        {
                            var point = value.GetPoint(i, j);
                            binaryWriter.Write((float)point.X);
                            binaryWriter.Write((float)point.Y);
                        }
                    }

                    writer.WriteString(context, Convert.ToBase64String(ms.ToArray()));
                    break;
                }

            default:
                throw new NotSupportedException($"Serialization format {format} not supported.");
        }
    }

    public static void WritePathCollection<TWriter>(this TWriter writer, object? context, PathCollection? value, SerializationFormat format = SerializationFormat.Compact) where TWriter : FormatWriterBase
    {
        if (value == null)
        {
            writer.WriteString(context: context, null);
            return;
        }

        switch (format)
        {
            case SerializationFormat.Underlying:
                {
                    var pathWriters = new List<TWriter>();

                    foreach (var path in value)
                    {
                        var polygonWriters = new List<TWriter>();

                        for (int i = 0; i < path.GetPolygonCount(); i++)
                        {
                            var pointWriters = new List<TWriter>();

                            for (int j = 0; j < path.GetPointCount(i); j++)
                            {
                                var point = path.GetPoint(i, j);
                                var pointWriter = (TWriter)writer.CreateWriter();
                                pointWriter.WriteSingleArray(new float[] { point.X, point.Y });
                                pointWriters.Add(pointWriter);
                            }

                            var polygonWriter = (TWriter)writer.CreateWriter();
                            polygonWriter.WriteArray(pointWriters.ToArray());
                            polygonWriters.Add(polygonWriter);
                        }

                        var pathWriter = (TWriter)writer.CreateWriter();
                        pathWriter.WriteArray(polygonWriters.ToArray());
                        pathWriters.Add(pathWriter);
                    }

                    writer.WriteArray(pathWriters.ToArray());
                    break;
                }

            case SerializationFormat.Compact:
                {
                    // Write as compact binary (Base64 for JSON/Text)
                    using var ms = new MemoryStream();
                    using var binaryWriter = new BinaryWriter(ms);

                    // Number of paths
                    binaryWriter.Write((int)value.Count);

                    foreach (var path in value)
                    {
                        // Number of polygons
                        binaryWriter.Write((int)path.GetPolygonCount());

                        for (int i = 0; i < path.GetPolygonCount(); i++)
                        {
                            // Number of points
                            binaryWriter.Write((int)path.GetPointCount(i));

                            for (int j = 0; j < path.GetPointCount(i); j++)
                            {
                                var point = path.GetPoint(i, j);
                                binaryWriter.Write((float)point.X);
                                binaryWriter.Write((float)point.Y);
                            }
                        }
                    }

                    writer.WriteString(context, Convert.ToBase64String(ms.ToArray()));
                    break;
                }

            default:
                throw new NotSupportedException($"Serialization format {format} not supported.");
        }
    }

    public static void WriteTexture2D<TWriter>(this TWriter writer, object? context, Texture2D? value, SerializationFormat format = SerializationFormat.Compact) where TWriter : FormatWriterBase
    {
        switch (format)
        {
            case SerializationFormat.Compact:
                using (var ms = new MemoryStream())
                {
                    if (value != null)
                    {
                        value.SaveAsPng(ms, value.Width, value.Height);
                        var base64_string = Convert.ToBase64String(ms.ToArray());
                        writer.WriteString(context, base64_string);
                    }
                    else
                    {
                        writer.WriteString(context, null);
                    }
                }
                break;

            case SerializationFormat.Underlying:
                using (var ms = new MemoryStream())
                {
                    if (value != null)
                    {
                        value.SaveAsPng(ms, value.Width, value.Height);
                        writer.WriteByteArray(context, ms.ToArray());
                    }
                    else
                    {
                        writer.WriteByteArray(context, []);
                    }
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    public static void WriteTieredScoreInt<TWriter>(this TWriter writer, object? context, TieredScore<int> value) where TWriter : FormatWriterBase
    {
        writer.WriteInt32Array(context, new int[] { value.Gold, value.Silver, value.Bronze });
    }

    public static void WriteTieredScoreSingle<TWriter>(this TWriter writer, object? context, TieredScore<float> value) where TWriter : FormatWriterBase
    {
        writer.WriteSingleArray(context, new float[] { value.Gold, value.Silver, value.Bronze });
    }

    public static void WriteTieredScoreTimeSpan<TWriter>(this TWriter writer, object? context, TieredScore<TimeSpan> value) where TWriter : FormatWriterBase
    {
        writer.WriteTimeSpanArray(context, new TimeSpan[] { value.Gold, value.Silver, value.Bronze });
    }

    public static void WriteAssetReference<TWriter, TAsset>(this TWriter writer, object? context, AssetReference<TAsset> value) where TWriter : FormatWriterBase where TAsset : class, IIdentifiable
    {
        writer.WriteString(value.ID);
    }

}
