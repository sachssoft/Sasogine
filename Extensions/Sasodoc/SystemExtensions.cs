using Sachssoft.Sasodoc;
using System;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public static class SystemExtensions
    {
        #region Uri
        public static Uri? ReadUri(this FormatReaderBase reader, string property, Uri? fallback)
        {
            var value = reader.ReadString(property, fallback?.ToString());
            return Uri.TryCreate(value, UriKind.Absolute, out var result) ? result : fallback;
        }

        public static void WriteUri(this FormatWriterBase writer, string property, Uri? value)
        {
            writer.WriteString(property, value?.ToString());
        }
        #endregion

        #region Version
        public static Version? ReadVersion(this FormatReaderBase reader, string property, Version? fallback)
        {
            var value = reader.ReadString(property, fallback?.ToString());
            return Version.TryParse(value, out var result) ? result : fallback;
        }

        public static void WriteVersion(this FormatWriterBase writer, string property, Version? value)
        {
            writer.WriteString(property, value?.ToString());
        }
        #endregion
    }
}
