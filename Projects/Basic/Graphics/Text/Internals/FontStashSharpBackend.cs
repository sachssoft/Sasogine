using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sachssoft.Engine.Graphics.Text;

/// <summary>Connects registered faces to the default FontStashSharp implementation.</summary>
internal sealed class FontStashSharpBackend : IFontBackend
{
    private static readonly ConditionalWeakTable<FontFace, FontSystem> MeasurementSystems = new();
    private static readonly ConditionalWeakTable<GraphicsDevice, FontStashDeviceFonts> DeviceFonts = new();
    private readonly FontRegistry _registry;

    internal FontStashSharpBackend(FontRegistry? registry = null) => _registry = registry ?? new();

    public void Register(string familyName, FontFace face)
    {
        ArgumentException.ThrowIfNullOrEmpty(familyName);
        ArgumentNullException.ThrowIfNull(face);
        FontFamily? family = _registry.GetFamily(familyName);
        if (family is null)
        {
            _registry.Register(familyName, face);
            return;
        }

        if (family.Faces.Any(existing => existing.WeightDefinition == face.WeightDefinition &&
            existing.StyleDefinition == face.StyleDefinition))
            throw new ArgumentException($"The font family '{familyName}' already contains this weight and style.", nameof(face));

        var updated = new FontFamily(family.Name, [.. family.Faces, face]) { IsMonospaced = family.IsMonospaced };
        _registry.Remove(family.Name);
        _registry.Register(updated);
    }

    public void Register(FontFamily fontFamily) => _registry.Register(fontFamily);

    public IEnumerable<string> GetFamilies() => _registry.FamilyNames;

    public IEnumerable<FontFace> GetFaces() => _registry.GetFaces();

    public FontFamily? GetFamily(string name) => _registry.GetFamily(name);

    public IFont GetFont(FontFace face, float size)
    {
        ArgumentNullException.ThrowIfNull(face);
        return face.GetFont(size);
    }

    internal static FontSystem GetMeasurementSystem(FontFace face) => MeasurementSystems.GetValue(face, CreateSystem);

    internal static SpriteFontBase GetRenderFont(GraphicsDevice device, FontFace face, float size)
    {
        ObjectDisposedException.ThrowIf(device.IsDisposed, device);
        return DeviceFonts.GetValue(device, static key => new FontStashDeviceFonts(key)).GetFont(face, size);
    }

    internal static FontSystem CreateSystem(FontFace face)
    {
        var system = new FontSystem();
        try
        {
            system.AddFont(face.FontBytes);
            return system;
        }
        catch
        {
            system.Dispose();
            throw;
        }
    }
}
