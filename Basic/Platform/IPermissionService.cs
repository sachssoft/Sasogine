using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Platform
{
    /// <summary>
    /// Platform service to handle runtime permissions on the device.
    /// 
    /// This is mainly relevant for mobile platforms (Android/iOS), where certain
    /// features require explicit user consent, e.g., accessing the camera, storage,
    /// location, or microphone. On desktop platforms, permissions are usually granted
    /// by default.
    /// 
    /// Example usage:
    /// <code>
    /// // Check if the app has storage permission
    /// if (!await permissionService.HasPermission("Storage"))
    /// {
    ///     // Request storage permission from the user
    ///     bool granted = await permissionService.RequestPermissionAsync("Storage");
    ///     if (!granted)
    ///     {
    ///         messageService.Show("Storage permission is required to save files.");
    ///     }
    /// }
    /// </code>
    /// 
    /// Example permission names:
    /// - "Camera"     → access to device camera
    /// - "Microphone" → access to microphone
    /// - "Location"   → access to GPS/location services
    /// - "Storage"    → read/write external files
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Requests a permission from the user.
        /// Returns true if the permission was granted, false otherwise.
        /// 
        /// Example:
        /// <code>
        /// bool cameraAllowed = await permissionService.RequestPermissionAsync("Camera");
        /// </code>
        /// </summary>
        Task<bool> RequestPermissionAsync(string permissionName);

        /// <summary>
        /// Checks if a permission is already granted.
        /// 
        /// Example:
        /// <code>
        /// if (permissionService.HasPermission("Location"))
        /// {
        ///     // Access GPS functionality
        /// }
        /// </code>
        /// </summary>
        bool HasPermission(string permissionName);
    }
}