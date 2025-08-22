using Sachssoft.Sasogine.Containers;
using System;
using System.IO;
using System.Text;

namespace Sachssoft.Sasogine.Resources.Wrappers
{
    /// <summary>
    /// Lazily wraps text data, either as a string or as a deferred stream source from a package,
    /// and converts it into a string when needed.
    /// </summary>
    public sealed class TextWrapper : PackageEntryWrapper<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextWrapper"/> class,
        /// wrapping the provided text data as a string.
        /// </summary>
        /// <param name="text">The string containing the text data.</param>
        public TextWrapper(string text)
            : base(text, typeof(string))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextWrapper"/> class,
        /// wrapping a deferred text data stream provider from the specified package.
        /// This allows deferred loading of text data to save memory.
        /// </summary>
        /// <param name="package">The package containing the text data.</param>
        /// <param name="streamProvider">
        /// A function that takes the package and returns a <see cref="Stream"/> containing the text data.
        /// </param>
        public TextWrapper(PackageBase package, Func<PackageBase, Stream?> streamProvider)
            : base(package, streamProvider)
        {
        }

        /// <summary>
        /// Transforms the wrapped content into a string.
        /// If the content is a string, it returns it directly.
        /// If the content is a deferred stream from a package, it reads the text on demand from the stream.
        /// </summary>
        /// <param name="wrapContent">
        /// The wrapped content to transform, expected to be either a <see cref="string"/> 
        /// or a tuple of (<see cref="PackageBase"/>, <see cref="Func{PackageBase, Stream}"/>).
        /// </param>
        /// <param name="sourceType">The type of the wrapped content.</param>
        /// <returns>The loaded string content, or <c>null</c> if the stream provider returns <c>null</c>.</returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the wrapped content type is not supported.
        /// </exception>
        protected override string? Transform(object? wrapContent, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                // Bereits ein String, direkt zurückgeben
                return wrapContent as string;
            }
            else if (IsPackageType(sourceType))
            {
                using var stream = OpenWrappedStream(wrapContent);
                if (stream == null)
                    return null;

                using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: false);
                return reader.ReadToEnd();
            }
            else
            {
                throw new NotSupportedException($"Unsupported source type: {sourceType.FullName}");
            }
        }
    }
}
