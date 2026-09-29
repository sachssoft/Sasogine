using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Engine
{
    /// <summary>
    /// Represents a generic engine object with identity, configuration,
    /// data context, and lifecycle support.
    /// </summary>
    public interface IEngineObject : IEngineReferenceable, IEngineObjectIdentityChanged
    {
        /// <summary>
        /// Gets the definition that configures the object.
        /// </summary>
        IDefinition Definition { get; }

        /// <summary>
        /// Gets a value indicating whether the object is loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Gets the class of the object.
        /// </summary>
        string? Class { get; }

        /// <summary>
        /// Gets or sets the custom data context associated with the object.
        /// </summary>
        object? DataContext { get; set; }

        /// <summary>
        /// Loads the object synchronously.
        /// </summary>
        void Load();

        /// <summary>
        /// Loads the object asynchronously.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the loading operation.
        /// </param>
        /// <returns>A task representing the asynchronous loading operation.</returns>
        Task LoadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Unloads the object and releases its associated resources.
        /// </summary>
        void Unload();
    }
}