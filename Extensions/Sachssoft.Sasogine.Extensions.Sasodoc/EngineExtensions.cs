using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public static class EngineExtensions
    {

        #region Insets
        public static Insets ReadInsets(this FormatReaderBase reader, string property, Insets fallback)
        {
            var rectReader = reader.Read(property);

            if (rectReader == null)
                return fallback;

            var left = rectReader.ReadSingle("Left", fallback.Left);
            var top = rectReader.ReadSingle("Top", fallback.Top);
            var right = rectReader.ReadSingle("Right", fallback.Right);
            var bottom = rectReader.ReadSingle("Bottom", fallback.Bottom);
            return (new Insets(left, top, right, top));
        }

        public static void WriteInsets(this FormatWriterBase writer, string property, Insets value)
        {
            var rectWriter = writer.CreateWriter();
            rectWriter.WriteSingle("Left", value.Left);
            rectWriter.WriteSingle("Top", value.Top);
            rectWriter.WriteSingle("Right", value.Right);
            rectWriter.WriteSingle("Bottom", value.Bottom);
            writer.Write(property, rectWriter);
        }
        #endregion
    }
}
