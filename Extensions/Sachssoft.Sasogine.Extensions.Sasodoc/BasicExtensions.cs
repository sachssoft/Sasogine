using Microsoft.Xna.Framework;
using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Graphics.Colors;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    // XNA Basic
    public static class BasicExtensions
    {

        #region Color
        public static Color ReadColor(this FormatReaderBase reader, string property, Color fallback)
        {
            var value = reader.ReadString(property, fallback.ToString());
            return ColorUtils.TryParse(value, out var result) ? result : fallback;
        }

        public static void WriteColor(this FormatWriterBase writer, string property, Color value)
        {
            writer.WriteString(property, value.ToString());
        }
        #endregion

        #region Vector2
        public static Vector2 ReadVector2(this FormatReaderBase reader, string property, Vector2 fallback)
        {
            var rectReader = reader.Read(property);

            if (rectReader == null)
                return fallback;

            var x = rectReader.ReadSingle(nameof(Vector2.X), fallback.X);
            var y = rectReader.ReadSingle(nameof(Vector2.Y), fallback.Y);
            return new Vector2(x, y);
        }

        public static void WriteVector2(this FormatWriterBase writer, string property, Vector2 value)
        {
            var rectWriter = writer.CreateWriter();
            rectWriter.WriteSingle(nameof(Vector2.X), value.X);
            rectWriter.WriteSingle(nameof(Vector2.Y), value.Y);
            writer.Write(property, rectWriter);
        }
        #endregion


        #region Vector3
        public static Vector3 ReadVector3(this FormatReaderBase reader, string property, Vector3 fallback)
        {
            var rectReader = reader.Read(property);

            if (rectReader == null)
                return fallback;

            var x = rectReader.ReadSingle(nameof(Vector3.X), fallback.X);
            var y = rectReader.ReadSingle(nameof(Vector3.Y), fallback.Y);
            var z = rectReader.ReadSingle(nameof(Vector3.Z), fallback.Z);
            return new Vector3(x, y, z);
        }

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
        public static Vector4 ReadVector4(this FormatReaderBase reader, string property, Vector4 fallback)
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
