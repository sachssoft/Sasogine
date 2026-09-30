using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Sachssoft.Engine.Services.Platform;
using System;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>
/// Provides Windows-specific device and display information.
/// </summary>
public sealed class WindowsDeviceInfoService : IDeviceInfoService
{
    private readonly Game _game;
    private string? _model;
    private string? _deviceId;
    private bool _deviceIdResolved;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDeviceInfoService"/> class.
    /// </summary>
    public WindowsDeviceInfoService(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    /// <inheritdoc />
    public string DeviceName => Environment.MachineName;

    /// <inheritdoc />
    public string OSVersion => Environment.OSVersion.VersionString;

    /// <inheritdoc />
    public string Model
    {
        get
        {
            if (_model is not null)
                return _model;

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                var manufacturer = (key?.GetValue("SystemManufacturer") as string)?.Trim();
                var productName = (key?.GetValue("SystemProductName") as string)?.Trim();

                if (!string.IsNullOrWhiteSpace(manufacturer) &&
                    !string.IsNullOrWhiteSpace(productName))
                {
                    _model = $"{manufacturer} {productName}";
                }
                else if (!string.IsNullOrWhiteSpace(productName))
                {
                    _model = productName;
                }
                else if (!string.IsNullOrWhiteSpace(manufacturer))
                {
                    _model = manufacturer;
                }
                else
                {
                    _model = "PC";
                }
            }
            catch
            {
                _model = "PC";
            }

            return _model;
        }
    }

    /// <inheritdoc />
    public int ScreenWidth => _game.Window.ClientBounds.Width;

    /// <inheritdoc />
    public int ScreenHeight => _game.Window.ClientBounds.Height;

    /// <inheritdoc />
    public float ScreenDpi
    {
        get
        {
            try
            {
                var hMonitor = WindowsNative.MonitorFromWindow(
                    _game.Window.Handle,
                    WindowsNative.MonitorDefaultToNearest);

                if (hMonitor == IntPtr.Zero)
                    return 96f;

                var result = WindowsNative.GetDpiForMonitor(
                    hMonitor,
                    WindowsNative.MdtEffectiveDpi,
                    out var dpiX,
                    out _);

                return result == 0 ? dpiX : 96f;
            }
            catch (DllNotFoundException)
            {
                return 96f;
            }
            catch (EntryPointNotFoundException)
            {
                return 96f;
            }
        }
    }

    /// <inheritdoc />
    public bool IsLandscape => ScreenWidth >= ScreenHeight;

    /// <inheritdoc />
    public string? DeviceId
    {
        get
        {
            if (_deviceIdResolved)
                return _deviceId;

            _deviceIdResolved = true;

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                _deviceId = (key?.GetValue("MachineGuid") as string)?.Trim();
            }
            catch
            {
                _deviceId = null;
            }

            return _deviceId;
        }
    }
}
