namespace Sachssoft.Sasogine.Services.Platform;

/// <summary>
/// Provides information about the current device and its display.
/// </summary>
public interface IDeviceInfoService
{
    /// <summary>
    /// Gets the friendly name of the device.
    /// </summary>
    string DeviceName { get; }

    /// <summary>
    /// Gets the operating system version.
    /// </summary>
    string OSVersion { get; }

    /// <summary>
    /// Gets the device model.
    /// </summary>
    string Model { get; }

    /// <summary>
    /// Gets the screen width in pixels.
    /// </summary>
    int ScreenWidth { get; }

    /// <summary>
    /// Gets the screen height in pixels.
    /// </summary>
    int ScreenHeight { get; }

    /// <summary>
    /// Gets the device DPI or pixel density.
    /// </summary>
    float ScreenDpi { get; }

    /// <summary>
    /// Gets a value indicating whether the device is in landscape orientation.
    /// </summary>
    bool IsLandscape { get; }

    /// <summary>
    /// Gets the optional device identifier, if available and permitted
    /// by the platform.
    /// </summary>
    string? DeviceId { get; }
}