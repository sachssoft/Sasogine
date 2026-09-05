using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Sources
{
    /// <summary>
    /// Defines a resource source that provides access to a file.
    /// </summary>
    public interface IFileSource
    {
        /// <summary>
        /// Gets or sets the path of the resource file.
        /// </summary>
        string? FilePath { get; set; }

        /// <summary>
        /// Gets a stream for reading the resource.
        /// </summary>
        /// <returns>
        /// A stream containing the resource data.
        /// </returns>
        Stream GetStream();

        /// <summary>
        /// Gets a stream asynchronously for reading the resource.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a stream
        /// with the resource data.
        /// </returns>
        Task<Stream> GetStreamAsync();
    }
}