using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Sources
{
    /// <summary>
    /// Provides access to resources embedded in an assembly.
    /// </summary>
    public sealed class EmbeddedResourceSource : ResourceSourceBase, IFileSource
    {
        private string[]? _cachedResourceNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceSource"/> class.
        /// </summary>
        public EmbeddedResourceSource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceSource"/> class
        /// with the specified file path.
        /// </summary>
        /// <param name="filePath">
        /// The path used to locate the embedded resource.
        /// </param>
        public EmbeddedResourceSource(string? filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceSource"/> class
        /// with the specified file path and assembly.
        /// </summary>
        /// <param name="filePath">
        /// The path used to locate the embedded resource.
        /// </param>
        /// <param name="assembly">
        /// The assembly containing the embedded resource.
        /// </param>
        public EmbeddedResourceSource(string? filePath, Assembly assembly)
        {
            FilePath = filePath;
            Assembly = assembly;
        }

        /// <summary>
        /// Gets or sets the assembly containing the embedded resource.
        /// </summary>
        public Assembly Assembly { get; set; } = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Gets or sets the path used to locate the embedded resource.
        /// </summary>
        public string? FilePath { get; set; }

        /// <inheritdoc/>
        protected override Stream OpenStream()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                throw new InvalidOperationException("FilePath is not set.");

            var resourceNames = _cachedResourceNames ??=
                Assembly.GetManifestResourceNames();

            string normalizedFile = NormalizeFilePath(FilePath);

            string? resourceName = resourceNames
                .FirstOrDefault(n =>
                    n.EndsWith(
                        normalizedFile,
                        StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                var availableResources =
                    string.Join(Environment.NewLine + "  ", resourceNames);

                throw new FileNotFoundException(
                    $"Embedded resource not found: {normalizedFile}{Environment.NewLine}" +
                    $"Available resources:{Environment.NewLine}  {availableResources}");
            }

            var stream = Assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
                throw new IOException(
                    $"Failed to open embedded resource stream: {resourceName}");

            return stream;
        }

        /// <inheritdoc/>
        protected override async Task<Stream> OpenStreamAsync()
        {
            using var originalStream = OpenStream();

            var memoryStream = new MemoryStream();

            await originalStream
                .CopyToAsync(memoryStream)
                .ConfigureAwait(false);

            memoryStream.Position = 0;
            return memoryStream;
        }

        private static string NormalizeFilePath(string filePath)
        {
            return filePath
                .Replace('/', '.')
                .Replace('\\', '.')
                .ToLowerInvariant();
        }
    }
}