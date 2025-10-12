using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Documents;
using Sachssoft.Observables;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Resources;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets;

public class Texture2DAsset : AssetBase<Texture2D>, ITypeRegistry
{
    static void ITypeRegistry.RegisterProperties(TypeRegistryContext context)
    {
        context.RegisterProperty(TextureTypeProperty);
        context.RegisterProperty(TranslationProperty);
        context.RegisterProperty(RotationProperty);
        context.RegisterProperty(ScaleProperty);
        context.RegisterProperty(PivotProperty);
        context.RegisterProperty(PatternProperty);
        context.RegisterProperty(PatternModeProperty);
        context.RegisterProperty(DiffuseColorProperty);
        context.RegisterProperty(OpacityProperty);
        context.RegisterProperty(LayerProperty);
        context.RegisterProperty(FilterModeProperty);
        context.RegisterProperty(AddressModeProperty);
        context.RegisterProperty(SpriteEffectProperty);
        context.RegisterProperty(BlendStateOverrideProperty);
        context.RegisterProperty(UseMipmapsProperty);
    }

    protected override Texture2D? Build(Stream stream)
    {
        // 1. Volltextur aus Stream laden
        Texture2D original = Texture2D.FromStream(GraphicsDevice, stream);

        if (!UseMipmaps)
        {
            // Keine MipMaps, direkt zurückgeben
            return original;
        }

        // 2. Neue Texture mit MipMap-Unterstützung anlegen
        int width = original.Width;
        int height = original.Height;
        int mipLevels = (int)Math.Floor(Math.Log(Math.Max(width, height), 2)) + 1;

        Texture2D texture = new Texture2D(GraphicsDevice, width, height, mipmap: true, SurfaceFormat.Color);

        // 3. Basislevel (Level 0) setzen
        Color[] pixels = new Color[width * height];
        original.GetData(pixels);
        texture.SetData(0, null, pixels, 0, pixels.Length);

        // 4. MipMap-Kette erstellen
        Texture2D currentLevel = original;
        for (int level = 1; level < mipLevels; level++)
        {
            width = Math.Max(width / 2, 1);
            height = Math.Max(height / 2, 1);

            // Pixel für nächstes Level generieren (Box-Downscale)
            Texture2D nextLevelTexture = Texture2DScaler.DownscaleBox(GraphicsDevice, currentLevel);
            Color[] mipPixels = new Color[width * height];
            nextLevelTexture.GetData(mipPixels);

            // In die Texture schreiben
            texture.SetData(level, null, mipPixels, 0, mipPixels.Length);

            currentLevel = nextLevelTexture;
        }

        return texture;
    }

    public SamplerState CreateSamplerState()
    {
        var state = new SamplerState
        {
            Filter = FilterMode switch
            {
                Texture2DFilterMode.Point => TextureFilter.Point,
                Texture2DFilterMode.Linear => TextureFilter.Linear,
                Texture2DFilterMode.Anisotropic => TextureFilter.Anisotropic,
                _ => TextureFilter.Point
            },
            AddressU = AddressMode switch
            {
                Texture2DAddressMode.Clamp => TextureAddressMode.Clamp,
                Texture2DAddressMode.Wrap => TextureAddressMode.Wrap,
                Texture2DAddressMode.Mirror => TextureAddressMode.Mirror,
                _ => TextureAddressMode.Clamp
            },
            AddressV = AddressMode switch
            {
                Texture2DAddressMode.Clamp => TextureAddressMode.Clamp,
                Texture2DAddressMode.Wrap => TextureAddressMode.Wrap,
                Texture2DAddressMode.Mirror => TextureAddressMode.Mirror,
                _ => TextureAddressMode.Clamp
            }
        };
        return state;
    }

    public GraphicsDevice? GraphicsDevice { get; set; }

    #region Properties

    public readonly static IProperty TextureTypeProperty =
        new StoredProperty<Texture2DAsset, Texture2DAssetType>(
            nameof(TextureType),
            defaultValue: Texture2DAssetType.Image,
            category: Observables.PropertyCategories.General,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadEnum<Texture2DAssetType>(context: p.Name),
                serialize: (p, w, v) => w.WriteEnum<Texture2DAssetType>(context: p.Name, (Texture2DAssetType)(v ?? Texture2DAssetType.Image)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Texture2DAssetType TextureType
    {
        get => GetValue<Texture2DAssetType>(TextureTypeProperty);
        set => SetValue<Texture2DAssetType>(TextureTypeProperty, value);
    }

    public readonly static IProperty TranslationProperty =
        new StoredProperty<Texture2DAsset, Vector2>(
            nameof(Translation),
            defaultValue: Vector2.Zero,
            category: Observables.PropertyCategories.Transform,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadVector2(context: p.Name),
                serialize: (p, w, v) => w.WriteVector2(context: p.Name, (Vector2)(v ?? Vector2.Zero)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Vector2 Translation
    {
        get => GetValue<Vector2>(TranslationProperty);
        set => SetValue(TranslationProperty, value);
    }

    public readonly static IProperty RotationProperty =
        new StoredProperty<Texture2DAsset, float>(
            nameof(Rotation),
            defaultValue: 0f,
            category: Observables.PropertyCategories.Transform,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadSingle(context: p.Name),
                serialize: (p, w, v) => w.WriteSingle(context: p.Name, (float)(v ?? 0f)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public float Rotation
    {
        get => GetValue<float>(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public readonly static IProperty ScaleProperty =
        new StoredProperty<Texture2DAsset, Vector2>(
            nameof(Scale),
            defaultValue: Vector2.One,
            category: Observables.PropertyCategories.Transform,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadVector2(context: p.Name),
                serialize: (p, w, v) => w.WriteVector2(context: p.Name, (Vector2)(v ?? Vector2.One)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Vector2 Scale
    {
        get => GetValue<Vector2>(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public readonly static IProperty PivotProperty =
        new StoredProperty<Texture2DAsset, Vector2>(
            nameof(Pivot),
            defaultValue: new Vector2(0.5f),
            category: Observables.PropertyCategories.Transform,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadVector2(context: p.Name),
                serialize: (p, w, v) => w.WriteVector2(context: p.Name, (Vector2)(v ?? new Vector2(0.5f))))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Vector2 Pivot
    {
        get => GetValue<Vector2>(PivotProperty);
        set => SetValue(PivotProperty, value);
    }

    public readonly static IProperty PatternProperty =
        new StoredProperty<Texture2DAsset, Texture2DPattern>(
            nameof(Pattern),
            defaultValue: Texture2DPattern.Stretch,
            category: Observables.PropertyCategories.Appearance,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadEnum<Texture2DPattern>(context: p.Name),
                serialize: (p, w, v) => w.WriteEnum<Texture2DPattern>(context: p.Name, (Texture2DPattern)(v ?? Texture2DPattern.Stretch)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Texture2DPattern Pattern
    {
        get => GetValue<Texture2DPattern>(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    public readonly static IProperty PatternModeProperty =
        new StoredProperty<Texture2DAsset, Texture2DPatternMode>(
            nameof(PatternMode),
            defaultValue: Texture2DPatternMode.Repeat,
            category: Observables.PropertyCategories.Appearance,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadEnum<Texture2DPatternMode>(context: p.Name),
                serialize: (p, w, v) => w.WriteEnum<Texture2DPatternMode>(context: p.Name, (Texture2DPatternMode)(v ?? Texture2DPatternMode.Repeat)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Texture2DPatternMode PatternMode
    {
        get => GetValue<Texture2DPatternMode>(PatternModeProperty);
        set => SetValue(PatternModeProperty, value);
    }

    public readonly static IProperty DiffuseColorProperty =
        new StoredProperty<Texture2DAsset, Color?>(
            nameof(DiffuseColor),
            defaultValue: Color.White,
            category: Observables.PropertyCategories.Appearance,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadColor(context: p.Name),
                serialize: (p, w, v) => w.WriteColor(context: p.Name, (Color)(v ?? Color.White)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Color? DiffuseColor
    {
        get => GetValue<Color?>(DiffuseColorProperty);
        set => SetValue(DiffuseColorProperty, value);
    }

    public readonly static IProperty OpacityProperty =
        new StoredProperty<Texture2DAsset, float>(
            nameof(Opacity),
            defaultValue: 1f,
            category: Observables.PropertyCategories.Appearance,
            coercion: (o, baseValue) => float.Clamp(baseValue, 0f, 1f),
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadSingle(context: p.Name),
                serialize: (p, w, v) => w.WriteSingle(context: p.Name, (float)(v ?? 1f)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public float Opacity
    {
        get => GetValue<float>(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    public readonly static IProperty LayerProperty =
        new StoredProperty<Texture2DAsset, int>(
            nameof(Layer),
            category: Observables.PropertyCategories.Rendering,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadInt32(context: p.Name),
                serialize: (p, w, v) => w.WriteInt32(context: p.Name, (int)(v ?? 0)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public int Layer
    {
        get => GetValue<int>(LayerProperty);
        set => SetValue(LayerProperty, value);
    }

    public readonly static IProperty FilterModeProperty =
        new StoredProperty<Texture2DAsset, Texture2DFilterMode>(
            nameof(FilterMode),
            category: Observables.PropertyCategories.Rendering,
            defaultValue: Texture2DFilterMode.Point,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadEnum<Texture2DFilterMode>(context: p.Name),
                serialize: (p, w, v) => w.WriteEnum<Texture2DFilterMode>(context: p.Name, (Texture2DFilterMode)(v ?? Texture2DFilterMode.Point)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Texture2DFilterMode FilterMode
    {
        get => GetValue<Texture2DFilterMode>(FilterModeProperty);
        set => SetValue<Texture2DFilterMode>(FilterModeProperty, value);
    }

    public readonly static IProperty AddressModeProperty =
        new StoredProperty<Texture2DAsset, Texture2DAddressMode>(
            nameof(AddressMode),
            defaultValue: Texture2DAddressMode.Clamp,
            category: Observables.PropertyCategories.Rendering,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadEnum<Texture2DAddressMode>(context: p.Name),
                serialize: (p, w, v) => w.WriteEnum<Texture2DAddressMode>(context: p.Name, (Texture2DAddressMode)(v ?? Texture2DAddressMode.Clamp)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Texture2DAddressMode AddressMode
    {
        get => GetValue<Texture2DAddressMode>(AddressModeProperty);
        set => SetValue<Texture2DAddressMode>(AddressModeProperty, value);
    }

    public readonly static IProperty SpriteEffectProperty =
        new StoredProperty<Texture2DAsset, SpriteEffects>(
            nameof(SpriteEffect),
            defaultValue: SpriteEffects.None,
            category: Observables.PropertyCategories.Rendering,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadEnum<SpriteEffects>(context: p.Name),
                serialize: (p, w, v) => w.WriteEnum<SpriteEffects>(context: p.Name, (SpriteEffects)(v ?? SpriteEffects.None)))
            {
                Visibility = PropertyVisibility.Collapsed
            });

    public SpriteEffects SpriteEffect
    {
        get => GetValue<SpriteEffects>(SpriteEffectProperty);
        set => SetValue<SpriteEffects>(SpriteEffectProperty, value);
    }

    public readonly static IProperty BlendStateOverrideProperty =
        new StoredProperty<Texture2DAsset, Texture2DBlendMode>(
            nameof(BlendStateOverride),
            defaultValue: Texture2DBlendMode.AlphaBlend,
            category: Observables.PropertyCategories.Rendering,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadEnum<Texture2DBlendMode>(context: p.Name),
                serialize: (p, w, v) => w.WriteEnum<Texture2DBlendMode>(context: p.Name, (Texture2DBlendMode)(v ?? Texture2DBlendMode.AlphaBlend)))
            {
                Visibility = PropertyVisibility.Visible
            });

    public Texture2DBlendMode BlendStateOverride
    {
        get => GetValue<Texture2DBlendMode>(BlendStateOverrideProperty);
        set => SetValue<Texture2DBlendMode>(BlendStateOverrideProperty, value);
    }

    public readonly static IProperty UseMipmapsProperty =
        new StoredProperty<Texture2DAsset, bool>(
            nameof(UseMipmaps),
            defaultValue: false,
            category: Observables.PropertyCategories.Rendering,
            metadata: new PropertySerializationMetadata(
                deserialize: (p, r) => r.ReadBoolean(context: p.Name),
                serialize: (p, w, v) => w.WriteBoolean(context: p.Name, (bool)(v ?? false)))
            {
                Visibility = PropertyVisibility.Collapsed
            });

    public bool UseMipmaps
    {
        get => GetValue<bool>(UseMipmapsProperty);
        set => SetValue(UseMipmapsProperty, value);
    }

    #endregion

    //#region Transformations

    //public Vector2 GetSize()
    //{
    //    if (Asset == null)
    //        return Vector2.Zero;

    //    return new Vector2(Asset.Width, Asset.Height);
    //}

    ///// <summary>
    ///// Transformationsmatrix: Translation, Rotation, Scale und Pivot
    ///// </summary>
    //public Matrix GetTransformMatrix()
    //{
    //    var size = GetSize();
    //    var pivotOffset = Pivot * size;

    //    return
    //        Matrix.CreateTranslation(new Vector3(-pivotOffset, 0f)) *
    //        Matrix.CreateScale(new Vector3(Scale, 1f)) *
    //        Matrix.CreateRotationZ(Rotation) *
    //        Matrix.CreateTranslation(new Vector3(Translation, 0f));
    //}

    //#endregion
}
