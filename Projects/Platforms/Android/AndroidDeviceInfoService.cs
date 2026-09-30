using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Util;
using Microsoft.Xna.Framework;
using Sachssoft.Engine.Services.Platform;
using System;

namespace Sachssoft.Engine.Platform.Android;

/// <summary>Provides Android-specific device and display information.</summary>
public sealed class AndroidDeviceInfoService : IDeviceInfoService
{
    private readonly Game _game;
    private string? _deviceId;
    private bool _deviceIdResolved;

    public AndroidDeviceInfoService(Game game) => _game = game ?? throw new ArgumentNullException(nameof(game));

    public string DeviceName => Build.Device ?? Build.Model ?? "Android";
    public string OSVersion => $"Android {Build.VERSION.Release}";
    public string Model => string.IsNullOrWhiteSpace(Build.Manufacturer) ? Build.Model ?? "Android" : $"{Build.Manufacturer} {Build.Model}".Trim();
    public int ScreenWidth => _game.Window.ClientBounds.Width;
    public int ScreenHeight => _game.Window.ClientBounds.Height;
    public float ScreenDpi => Application.Context.Resources?.DisplayMetrics?.DensityDpi ?? (float)DisplayMetricsDensity.Default;
    public bool IsLandscape => ScreenWidth >= ScreenHeight;

    public string? DeviceId
    {
        get
        {
            if (_deviceIdResolved)
                return _deviceId;
            _deviceIdResolved = true;
            try
            {
                _deviceId = Settings.Secure.GetString(Application.Context.ContentResolver, Settings.Secure.AndroidId);
            }
            catch
            {
                _deviceId = null;
            }
            return _deviceId;
        }
    }
}
