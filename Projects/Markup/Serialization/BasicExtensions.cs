using Sachssoft.Sasogine.Graphics;
using Microsoft.Xna.Framework;
using Sachssoft.Sasodoc;

namespace Sachssoft.Sasogine.Markup.Serialization
{
    // XNA Basic
    /// <summary>
    /// Provides markup serialization extensions for common MonoGame value types.
    /// </summary>
    public static class BasicExtensions
    {

        #region Color
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
        /// Writes a Color value to the specified markup property.
        /// </summary>
        /// <param name="writer">The markup writer.</param>
        /// <param name="property">The property name.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="alpha">A value indicating whether the alpha channel is included.</param>
        public static void WriteColor(this FormatWriterBase writer, string property, Color value, bool alpha = true)
        {
            writer.WriteString(property, ColorUtils.ToHexString(value, alpha));
        }
        #endregion

        #region Vector2
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
        /// Writes a Vector2 value to the specified markup property.
        /// </summary>
        /// <param name="writer">The markup writer.</param>
        /// <param name="property">The property name.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteVector2(this FormatWriterBase writer, string property, Vector2 value)
        {
            var rectWriter = writer.CreateWriter();
            rectWriter.WriteSingle(nameof(Vector2.X), value.X);
            rectWriter.WriteSingle(nameof(Vector2.Y), value.Y);
            writer.Write(property, rectWriter);
        }
        #endregion


        #region Vector3
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
        /// Writes a Vector3 value to the specified markup property.
        /// </summary>
        /// <param name="writer">The markup writer.</param>
        /// <param name="property">The property name.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteVector3(this FormatWriterBase writer, string property, Vector3 value)
        {
            var rectWriter = writer.CreateWriter();
            rectWriter.WriteSingle(nameof(Vector3.X), value.X);
            rectWriter.WriteSingle(nameof(Vector3.Y), value.Y);
            rectWriter.WriteSingle(nameof(Vector3.Z), value.Z);
            writer.Write(property, rectWriter);
        }
        #endregion


        #region Vector4
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

        /// <summary>
        /// Writes a Vector4 value to the specified markup property.
        /// </summary>
        /// <param name="writer">The markup writer.</param>
        /// <param name="property">The property name.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteVector4(this FormatWriterBase writer, string property, Vector4 value)
        {
            var rectWriter = writer.CreateWriter();
            rectWriter.WriteSingle(nameof(Vector4.X), value.X);
            rectWriter.WriteSingle(nameof(Vector4.Y), value.Y);
            rectWriter.WriteSingle(nameof(Vector4.Z), value.Z);
            rectWriter.WriteSingle(nameof(Vector4.W), value.W);
            writer.Write(property, rectWriter);
        }
        #endregion
    }
}
