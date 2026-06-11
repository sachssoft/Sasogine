using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Sources
{
    public sealed class EmbeddedResourceSource : ResourceSourceBase, IFileSource
    {
        // Cache der Ressourcen-Namen für Performance
        private string[]? _cachedResourceNames;

        public EmbeddedResourceSource() { }

        public EmbeddedResourceSource(string? filePath)
        {
            FilePath = filePath;
        }

        public EmbeddedResourceSource(string? filePath, Assembly assembly)
        {
            FilePath = filePath;
            Assembly = assembly;
        }

        public Assembly Assembly { get; set; } = Assembly.GetExecutingAssembly();

        public string? FilePath { get; set; }

        protected override Stream OpenStream()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                throw new InvalidOperationException("FilePath is not set.");
            if (Assembly == null)
                throw new InvalidOperationException("Assembly is not set.");

            var resourceNames = _cachedResourceNames ??= Assembly.GetManifestResourceNames();
            string normalizedFile = NormalizeFilePath(FilePath);

            string? resourceName = resourceNames
                .FirstOrDefault(n => n.EndsWith(normalizedFile, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                var availableResources = string.Join(Environment.NewLine + "  ", resourceNames);
                throw new FileNotFoundException(
                    $"Embedded resource not found: {normalizedFile}{Environment.NewLine}" +
                    $"Available resources:{Environment.NewLine}  {availableResources}");
            }

            var stream = Assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new IOException($"Failed to open embedded resource stream: {resourceName}");

            return stream;
        }

        protected override async Task<Stream> OpenStreamAsync()
        {
            var originalStream = OpenStream();
            var memoryStream = new MemoryStream();
            await originalStream.CopyToAsync(memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private static string NormalizeFilePath(string filePath)
        {
            return filePath.Replace('/', '.').Replace('\\', '.').ToLowerInvariant();
        }
    }
}
