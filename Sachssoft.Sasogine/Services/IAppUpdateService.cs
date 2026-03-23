using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Platform
{
    /// <summary>
    /// Platform service to check for application updates and apply them if available.
    /// </summary>
    public interface IAppUpdateService
    {
        /// <summary>
        /// Checks whether an update is available.
        /// </summary>
        /// <returns>True if an update is available, false otherwise.</returns>
        Task<bool> CheckForUpdatesAsync();

        /// <summary>
        /// Applies the update if available.
        /// </summary>
        Task ApplyUpdatesAsync();

        /// <summary>
        /// Indicates whether there is a pending update.
        /// </summary>
        bool HasPendingUpdate { get; }
    }
}