using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using sachssoft.Sasogine.Elements;

namespace sachssoft.Sasogine.Resources;

public class Texture2DAsset : AssetProvider<Texture2D>
{
    private GraphicsDevice? _graphics_device;
    private string? _filename;
    private Color? _diffuse_color = null;
    private Vector2 _translation = Vector2.Zero;
    private float _rotation = 0f; // intern in Radiant
    private Vector2 _scale = Vector2.One;
    private Texture2DPatterns _pattern;
    private bool _is_loaded = false;

    public Texture2DAsset()
    {
    }

    public Texture2DAsset(string filename) : base()
    {
        _filename = filename;
    }

    public Texture2DAsset(string? id, string filename) : base(id)
    {
        _filename = filename;
    }

    public Vector2 GetSize()
    {
        if (Asset == null)
            return Vector2.Zero;
        return new Vector2(Asset.Width, Asset.Height);
    }

    /// <summary>
    /// Gibt die Transformationsmatrix zurück, die Translation, Rotation und Scale kombiniert.
    /// </summary>
    public Matrix GetTransformMatrix()
    {
        var size = GetSize();
        return
            Matrix.CreateTranslation(new Vector3(-size / 2f, 0f)) *
            Matrix.CreateScale(new Vector3(_scale, 1f)) *
            Matrix.CreateRotationZ(_rotation) *
            Matrix.CreateTranslation(new Vector3(_translation, 0f));
    }

    public static void RaiseChange(
        IAssetCollectionProvider? provider,
        ref Texture2DAsset? asset,
        ref Association<Texture2DAsset>? field,
        Association<Texture2DAsset>? value,
        PropertyChangedEventHandler? changed_event = null,
        Action<Texture2DAsset>? load = null,
        Action<Texture2DAsset>? unload = null)
    {
        RaiseChange<Texture2DAsset, Texture2D>(provider, ref asset, ref field, value, changed_event, load, unload);
    }

    public string? Filename
    {
        get => _filename;
        set => RaiseAndSetIfChanged(ref _filename, value);
    }

    public Texture2DPatterns Pattern
    {
        get => _pattern;
        set => RaiseAndSetIfChanged(ref _pattern, value);
    }

    public Vector2 Translation
    {
        get => _translation;
        set => RaiseAndSetIfChanged(ref _translation, value);
    }

    /// <summary>
    /// Rotation in Grad (intern wird Radiant gespeichert)
    /// </summary>
    public float Rotation
    {
        get => MathHelper.ToDegrees(_rotation);
        set => RaiseAndSetIfChanged(ref _rotation, MathHelper.ToRadians(value));
    }

    public Vector2 Scale
    {
        get => _scale;
        set => RaiseAndSetIfChanged(ref _scale, value);
    }

    public Color? DiffuseColor
    {
        get => _diffuse_color;
        set => RaiseAndSetIfChanged(ref _diffuse_color, value);
    }

    public void Load(GraphicsDevice graphics_device, string path)
    {
        if (_is_loaded)
            return;

        _graphics_device = graphics_device ?? throw new ArgumentNullException(nameof(graphics_device));
        IsError = false;

        try
        {
            var fullPath = System.IO.Path.Combine(path ?? "", _filename ?? "");
            if (!System.IO.File.Exists(fullPath))
                throw new System.IO.FileNotFoundException("Texture file not found", fullPath);

            Asset = Texture2D.FromFile(_graphics_device, fullPath);
        }
        catch (Exception ex)
        {
            IsError = true;
            System.Diagnostics.Debug.WriteLine($"[Texture2DAsset.Load] Fehler: {ex.Message}");
        }

        _is_loaded = true;
    }

    public void Unload()
    {
        if (!_is_loaded)
            return;

        try
        {
            Asset?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Texture2DAsset.Unload] Fehler: {ex.Message}");
        }

        Asset = null;
        _is_loaded = false;
    }
}
