using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using sachssoft.Sasogine.Graphics.Colors;
using System.IO;
using System;
using System.Threading;

namespace sachssoft.Sasogine.Document;

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

    public static Texture2D? ReadTexture2D<TReader>(this TReader reader, string property, GraphicsDevice graphics_device, Texture2D? fallback = default, SerializationFormat format = SerializationFormat.Base64) where TReader : FormatReaderBase
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadTexture2D(reader: reader, context: property, fallback: fallback, graphics_device: graphics_device);
    }

    public static Texture2D? ReadTexture2D<TReader>(this TReader reader, object? context, GraphicsDevice graphics_device, Texture2D? fallback = default, SerializationFormat format = SerializationFormat.Base64) where TReader : FormatReaderBase
    {
        switch (format)
        {
            case SerializationFormat.Base64:
                {
                    var base64_string = reader.ReadString(context, string.Empty);
                    if (string.IsNullOrEmpty(base64_string))
                        return fallback;

                    var texture_bytes = Convert.FromBase64String(base64_string);
                    using var ms = new MemoryStream(texture_bytes);
                    return Texture2D.FromStream(graphics_device, ms);
                }

            case SerializationFormat.ByteArray:
                {
                    var texture_bytes = reader.ReadByteArray(context, null);
                    if (texture_bytes == null || texture_bytes.Length == 0)
                        return fallback;

                    using var ms = new MemoryStream(texture_bytes);
                    return Texture2D.FromStream(graphics_device, ms);
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
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

    public static void WriteTexture2D<TWriter>(this TWriter writer, object? context, Texture2D? value, SerializationFormat format = SerializationFormat.Base64) where TWriter : FormatWriterBase
    {
        switch (format)
        {
            case SerializationFormat.Base64:
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

            case SerializationFormat.ByteArray:
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

}
