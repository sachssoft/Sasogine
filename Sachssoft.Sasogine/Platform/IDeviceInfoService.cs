namespace Sachssoft.Sasogine.Platform
{
    public interface IDeviceInfoService
    {
        /// <summary>Friendly name of the device (e.g., "John's iPhone").</summary>
        string DeviceName { get; }

        /// <summary>OS version string (e.g., "iOS 17.1" / "Android 14").</summary>
        string OSVersion { get; }

        /// <summary>Device model (e.g., "iPhone 14 Pro", "Samsung Galaxy S23").</summary>
        string Model { get; }

        /// <summary>Screen width in pixels.</summary>
        int ScreenWidth { get; }

        /// <summary>Screen height in pixels.</summary>
        int ScreenHeight { get; }

        /// <summary>Device DPI / pixel density.</summary>
        float ScreenDpi { get; }

        /// <summary>Is the device in landscape orientation (true) or portrait (false).</summary>
        bool IsLandscape { get; }

        /// <summary>Optional unique device identifier (if permitted).</summary>
        string? DeviceId { get; }
    }
}