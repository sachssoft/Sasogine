using Sachssoft.Sasogine.Graphics;
using Microsoft.Xna.Framework;
using Sachssoft.Sasodoc;

namespace Sachssoft.Sasogine.Documents.Serialization
{
    /// <summary>
    /// Provides markup serialization extensions for reading Basic values.
    /// </summary>
    public static partial class FormatWriterExtensions
    {

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
    }
}
