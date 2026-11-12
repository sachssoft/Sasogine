using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Graphics.Text;

internal sealed class FontStashDeviceFonts
{
    private readonly GraphicsDevice _device;
    private readonly Dictionary<FontFace, FontSystem> _systems = new(ReferenceEqualityComparer.Instance);

    internal FontStashDeviceFonts(GraphicsDevice device)
    {
        _device = device;
        device.DeviceResetting += OnDeviceResetting;
        device.Disposing += OnDeviceDisposing;
    }

    internal SpriteFontBase GetFont(FontFace face, float size)
    {
        if (!_systems.TryGetValue(face, out FontSystem? system))
        {
            system = FontStashSharpBackend.CreateSystem(face);
            _systems.Add(face, system);
        }
        return system.GetFont(size);
    }

    private void OnDeviceResetting(object? sender, EventArgs e) => Clear();

    private void OnDeviceDisposing(object? sender, EventArgs e)
    {
        _device.DeviceResetting -= OnDeviceResetting;
        _device.Disposing -= OnDeviceDisposing;
        Clear();
    }

    private void Clear()
    {
        foreach (FontSystem system in _systems.Values)
            system.Dispose();
        _systems.Clear();
    }
}
