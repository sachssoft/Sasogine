using System;
using System.Management;
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Platform.Windows
{
    /// <summary>
    /// Fully implemented MonoGame Windows device info service.
    /// Provides device name, OS version, model, screen info, DPI, orientation, and optional unique ID.
    /// </summary>
    public class WindowsDeviceInfoService : IDeviceInfoService
    {
        private readonly Game _game;
        private string? _model;
        private string? _deviceId;

        /// <summary>
        /// Constructor with MonoGame Game instance.
        /// </summary>
        /// <param name="game">MonoGame Game instance.</param>
        // Konstruktor mit MonoGame Game-Instanz
        public WindowsDeviceInfoService(Game game)
        {
            _game = game;
        }

        /// <summary>Friendly device name (PC name).</summary>
        // Freundlicher Gerätename (Computername)
        public string DeviceName => Environment.MachineName;

        /// <summary>OS version string.</summary>
        // Betriebssystem-Version als String
        public string OSVersion => Environment.OSVersion.VersionString;

        /// <summary>Device model (e.g., "Dell XPS 15 9520").</summary>
        // Geräte-Modell (z. B. Laptop/PC Modell)
        public string Model
        {
            get
            {
                if (_model != null) return _model;

                try
                {
                    // WMI-Abfrage für Hersteller und Modell
                    using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
                    foreach (var obj in searcher.Get())
                    {
                        var manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "Unknown";
                        var model = obj["Model"]?.ToString()?.Trim() ?? "Unknown";
                        _model = $"{manufacturer} {model}";
                        break;
                    }
                }
                catch
                {
                    _model = "PC"; // Fallback
                }

                return _model;
            }
        }

        /// <summary>Screen width in pixels.</summary>
        // Bildschirmbreite in Pixel
        public int ScreenWidth => _game.Window.ClientBounds.Width;

        /// <summary>Screen height in pixels.</summary>
        // Bildschirmhöhe in Pixel
        public int ScreenHeight => _game.Window.ClientBounds.Height;

        /// <summary>
        /// Gets the DPI of the monitor where the game window is displayed.
        /// Falls back to 96 DPI on Windows 7.
        /// </summary>
        // Liefert die DPI des Monitors, auf dem das Spiel-Fenster liegt
        // Multi-Monitor korrekt auf Windows 8.1+, Fallback 96 auf Windows 7
        public float ScreenDpi
        {
            get
            {
                try
                {
                    var osVersion = Environment.OSVersion.Version;
                    if (osVersion.Major > 6 || (osVersion.Major == 6 && osVersion.Minor >= 3))
                    {
                        // Windows 8.1+ -> GetDpiForMonitor
                        var hWnd = _game.Window.Handle;
                        var hMonitor = WindowsNative.MonitorFromWindow(hWnd, WindowsNative.MONITOR_DEFAULTTONEAREST);
                        WindowsNative.GetDpiForMonitor(hMonitor, WindowsNative.MDT_EFFECTIVE_DPI, out uint dpiX, out _);
                        return dpiX;
                    }
                    else
                    {
                        // Windows 7 -> Fallback
                        return 96f;
                    }
                }
                catch
                {
                    return 96f; // fallback
                }
            }
        }

        /// <summary>True if the display is landscape.</summary>
        // Gibt an, ob das Display im Querformat ist
        public bool IsLandscape => ScreenWidth >= ScreenHeight;

        /// <summary>Optional unique device ID from WMI.</summary>
        // Optional: Eindeutige Geräte-ID über WMI
        public string? DeviceId
        {
            get
            {
                if (_deviceId != null) return _deviceId;

                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
                    foreach (var obj in searcher.Get())
                    {
                        var uuid = obj["UUID"]?.ToString()?.Trim();
                        // Ungültige UUID ignorieren
                        if (!string.IsNullOrEmpty(uuid) && uuid != "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
                        {
                            _deviceId = uuid;
                            break;
                        }
                    }
                }
                catch
                {
                    _deviceId = null; // fallback
                }

                return _deviceId;
            }
        }
    }
}