using Microsoft.Xna.Framework;
using Sachssoft.Engine.Services.Platform;
using System;
using UIKit;

namespace Sachssoft.Engine.Platform.iOS;

/// <summary>Provides iOS-specific device and display information.</summary>
public sealed class iOSDeviceInfoService : IDeviceInfoService
{
    private readonly Game _game;

    public iOSDeviceInfoService(Game game) => _game = game ?? throw new ArgumentNullException(nameof(game));

    public string DeviceName => UIDevice.CurrentDevice.Name;
    public string OSVersion => $"iOS {UIDevice.CurrentDevice.SystemVersion}";
    public string Model => UIDevice.CurrentDevice.Model;
    public int ScreenWidth => _game.Window.ClientBounds.Width;
    public int ScreenHeight => _game.Window.ClientBounds.Height;
    public float ScreenDpi => (float)(UIScreen.MainScreen.Scale * 163.0);
    public bool IsLandscape => ScreenWidth >= ScreenHeight;
    public string? DeviceId => UIDevice.CurrentDevice.IdentifierForVendor?.AsString();
}
