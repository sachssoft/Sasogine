using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Observables;
using Sachssoft.Runtime;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Resources;
using System;

namespace Sachssoft.Sasogine.Assets;

public class Texture2DAsset : AssetProvider<Texture2D>
{
    private GraphicsDevice? _graphicsDevice;
    private string? _filename;
    private bool _isLoaded = false;

    public Texture2DAsset() { }

    public Texture2DAsset(string filename)
    {
        _filename = filename;
    }

    public Texture2DAsset(string? id, string filename) : base(id)
    {
        _filename = filename;
    }

    protected override void OnTypeRegistering(TypeRegisteringContext context)
    {
        base.OnTypeRegistering(context);

        context.RegisterProperty(PatternProperty);
        context.RegisterProperty(TranslationProperty);
        context.RegisterProperty(RotationProperty);
        context.RegisterProperty(ScaleProperty);
        context.RegisterProperty(PivotProperty);
        context.RegisterProperty(DiffuseColorProperty);
        context.RegisterProperty(OpacityProperty);
        context.RegisterProperty(LayerProperty);
        context.RegisterProperty(WrapModeProperty);
        context.RegisterProperty(FilterModeProperty);
    }

    #region Properties

    public readonly static IProperty PatternProperty =
        new StoredProperty<Texture2DAsset, Texture2DPatterns>(
            nameof(Pattern),
            category: Observables.PropertyCategories.Appearance);
    public Texture2DPatterns Pattern
    {
        get => GetValue<Texture2DPatterns>(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    public readonly static IProperty TranslationProperty =
        new StoredProperty<Texture2DAsset, Vector2>(
            nameof(Translation),
            category: Observables.PropertyCategories.Transform);
    public Vector2 Translation
    {
        get => GetValue<Vector2>(TranslationProperty);
        set => SetValue(TranslationProperty, value);
    }

    public readonly static IProperty RotationProperty =
        new StoredProperty<Texture2DAsset, float>(
            nameof(Rotation),
            category: Observables.PropertyCategories.Transform);
    public float Rotation
    {
        get => GetValue<float>(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public readonly static IProperty ScaleProperty =
        new StoredProperty<Texture2DAsset, Vector2>(
            nameof(Scale),
            defaultValue: Vector2.One,
            category: Observables.PropertyCategories.Transform);
    public Vector2 Scale
    {
        get => GetValue<Vector2>(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public readonly static IProperty PivotProperty =
        new StoredProperty<Texture2DAsset, Vector2>(
            nameof(Pivot),
            defaultValue: new Vector2(0.5f),
            category: Observables.PropertyCategories.Transform);
    public Vector2 Pivot
    {
        get => GetValue<Vector2>(PivotProperty);
        set => SetValue(PivotProperty, value);
    }

    public readonly static IProperty DiffuseColorProperty =
        new StoredProperty<Texture2DAsset, Color?>(
            nameof(DiffuseColor),
            defaultValue: Color.White,
            category: Observables.PropertyCategories.Appearance);
    public Color? DiffuseColor
    {
        get => GetValue<Color?>(DiffuseColorProperty);
        set => SetValue(DiffuseColorProperty, value);
    }

    public readonly static IProperty OpacityProperty =
        new StoredProperty<Texture2DAsset, float>(
            nameof(Opacity),
            defaultValue: 1f,
            category: Observables.PropertyCategories.Appearance);
    public float Opacity
    {
        get => GetValue<float>(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    public readonly static IProperty LayerProperty =
        new StoredProperty<Texture2DAsset, int>(
            nameof(Layer),
            category: Observables.PropertyCategories.Rendering);
    public int Layer
    {
        get => GetValue<int>(LayerProperty);
        set => SetValue(LayerProperty, value);
    }

    /// <example>
    ///SamplerState sampler = new SamplerState
    ///{
    ///    AddressU = wrapMode switch
    ///    {
    ///        TextureWrapMode.Repeat => TextureAddressMode.Wrap,
    ///        TextureWrapMode.Clamp => TextureAddressMode.Clamp,
    ///        TextureWrapMode.Mirror => TextureAddressMode.Mirror,
    ///        _ => TextureAddressMode.Wrap
    ///    },
    ///    AddressV = wrapMode switch
    ///    {
    ///        TextureWrapMode.Repeat => TextureAddressMode.Wrap,
    ///        TextureWrapMode.Clamp => TextureAddressMode.Clamp,
    ///        TextureWrapMode.Mirror => TextureAddressMode.Mirror,
    ///        _ => TextureAddressMode.Wrap
    ///    }
    ///};
    ///</example>

    public readonly static IProperty WrapModeProperty =
        new StoredProperty<Texture2DAsset, TextureWrapMode>(
            nameof(WrapMode),
            category: Observables.PropertyCategories.Appearance);
    public TextureWrapMode WrapMode
    {
        get => GetValue<TextureWrapMode>(WrapModeProperty);
        set => SetValue(WrapModeProperty, value);
    }

    public readonly static IProperty FilterModeProperty =
        new StoredProperty<Texture2DAsset, TextureFilterMode>(
            nameof(FilterMode),
            category: Observables.PropertyCategories.Appearance);
    public TextureFilterMode FilterMode
    {
        get => GetValue<TextureFilterMode>(FilterModeProperty);
        set => SetValue(FilterModeProperty, value);
    }

    #endregion

    #region Transformations

    public Vector2 GetSize()
    {
        if (Asset == null)
            return Vector2.Zero;

        return new Vector2(Asset.Width, Asset.Height);
    }

    /// <summary>
    /// Transformationsmatrix: Translation, Rotation, Scale und Pivot
    /// </summary>
    public Matrix GetTransformMatrix()
    {
        var size = GetSize();
        var pivotOffset = Pivot * size;

        return
            Matrix.CreateTranslation(new Vector3(-pivotOffset, 0f)) *
            Matrix.CreateScale(new Vector3(Scale, 1f)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateTranslation(new Vector3(Translation, 0f));
    }

    #endregion

    #region Load / Unload

    public void Load(GraphicsDevice graphicsDevice, string path)
    {
        if (_isLoaded) return;

        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        IsError = false;

        try
        {
            var fullPath = System.IO.Path.Combine(path ?? "", _filename ?? "");
            if (!System.IO.File.Exists(fullPath))
                throw new System.IO.FileNotFoundException("Texture file not found", fullPath);

            Asset = Texture2D.FromFile(_graphicsDevice, fullPath);
        }
        catch (Exception ex)
        {
            IsError = true;
            System.Diagnostics.Debug.WriteLine($"[Texture2DAsset.Load] Fehler: {ex.Message}");
        }

        _isLoaded = true;
    }

    public void Unload()
    {
        if (!_isLoaded) return;

        try
        {
            Asset?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Texture2DAsset.Unload] Fehler: {ex.Message}");
        }

        Asset = null;
        _isLoaded = false;
    }

    #endregion
}
