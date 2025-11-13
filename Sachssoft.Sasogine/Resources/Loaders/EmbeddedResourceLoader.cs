using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sachssoft.Sasogine.Resources
{
    /// <summary>
    /// Loads a file embedded as a resource inside a .NET assembly.
    /// Automatically converts file paths to resource names.
    /// </summary>
    public sealed class EmbeddedResourceLoader : LoaderBase
    {
        private readonly Assembly _assembly;

        /// <summary>
        /// Initializes a new instance of <see cref="EmbeddedResourceLoader"/> for the specified embedded resource path.
        /// </summary>
        /// <param name="filePath">The relative path of the embedded resource, e.g., "Assets/UI/default_ui_skin.xml".</param>
        /// <param name="assembly">The assembly containing the embedded resource. Defaults to the executing assembly if not specified.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> or <paramref name="assembly"/> is null.</exception>
        public EmbeddedResourceLoader(string filePath, Assembly assembly)
            : base(filePath)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EmbeddedResourceLoader"/> using the executing assembly.
        /// </summary>
        /// <param name="filePath">The relative path of the embedded resource.</param>
        public EmbeddedResourceLoader(string filePath)
            : this(filePath, Assembly.GetExecutingAssembly())
        {
        }

        /// <summary>
        /// Checks whether the embedded resource exists in the assembly.
        /// </summary>
        public override bool IsFileExist
        {
            get
            {
                string normalizedFile = NormalizeFilePath(FilePath);
                return _assembly.GetManifestResourceNames()
                                .Any(n => n.EndsWith(normalizedFile, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Opens a readable <see cref="Stream"/> to the embedded resource.
        /// </summary>
        /// <returns>A readable <see cref="Stream"/> of the embedded resource.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the resource does not exist in the assembly.</exception>
        /// <exception cref="IOException">Thrown if the resource stream cannot be opened.</exception>
        protected override Stream StreamOpen()
        {
            string normalizedFile = NormalizeFilePath(FilePath);

            string? resourceName = _assembly.GetManifestResourceNames()
                                            .FirstOrDefault(n => n.EndsWith(normalizedFile, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                var availableResources = string.Join(Environment.NewLine + "  ", _assembly.GetManifestResourceNames());
                throw new FileNotFoundException(
                    $"Embedded resource not found: {normalizedFile}{Environment.NewLine}" +
                    $"Available resources:{Environment.NewLine}  {availableResources}");
            }

            var stream = _assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new IOException($"Failed to open embedded resource stream: {resourceName}");

            return stream;
        }

        /// <summary>
        /// Converts a relative file path to the corresponding embedded resource name.
        /// Replaces directory separators ('/' or '\') with dots ('.').
        /// </summary>
        /// <param name="filePath">The file path to normalize.</param>
        /// <returns>The normalized resource name.</returns>
        private static string NormalizeFilePath(string filePath)
        {
            return filePath.Replace('/', '.').Replace('\\', '.');
        }
    }
}
