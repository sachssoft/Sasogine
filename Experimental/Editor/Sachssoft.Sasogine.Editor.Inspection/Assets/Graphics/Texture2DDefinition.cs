using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Documents;
using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Inspection.Assets.Graphics
{
    public class Texture2DDefinition : AssetDefinition, ITexture2DDefinition, ITypeRegistry
    {
        static void ITypeRegistry.RegisterProperties(TypeRegistryContext context)
        {
            context.RegisterProperty(AssetTypeProperty);
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
            context.RegisterProperty(BlendModeProperty);
            context.RegisterProperty(UseMipmapsProperty);
        }

        public Texture2DDefinition()
        {
        }

        public readonly static IProperty AssetTypeProperty =
            new StoredProperty<Texture2DDefinition, Texture2DAssetType>(
                nameof(AssetType),
                defaultValue: Texture2DAssetType.Image,
                category: Observables.PropertyCategories.General,
                metadata: new PropertySerializationMetadata(
                    deserialize: (p, r) => r.ReadEnum<Texture2DAssetType>(context: p.Name),
                    serialize: (p, w, v) => w.WriteEnum<Texture2DAssetType>(context: p.Name, (Texture2DAssetType)(v ?? Texture2DAssetType.Image)))
                {
                    Visibility = PropertyVisibility.Visible
                });

        public Texture2DAssetType AssetType
        {
            get => GetValue<Texture2DAssetType>(AssetTypeProperty);
            set => SetValue<Texture2DAssetType>(AssetTypeProperty, value);
        }

        public readonly static IProperty TranslationProperty =
            new StoredProperty<Texture2DDefinition, Vector2>(
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
            new StoredProperty<Texture2DDefinition, float>(
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
            new StoredProperty<Texture2DDefinition, Vector2>(
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
            new StoredProperty<Texture2DDefinition, Vector2>(
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
            new StoredProperty<Texture2DDefinition, Texture2DPattern>(
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
            new StoredProperty<Texture2DDefinition, Texture2DPatternMode>(
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
            new StoredProperty<Texture2DDefinition, Color?>(
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
            new StoredProperty<Texture2DDefinition, float>(
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
            new StoredProperty<Texture2DDefinition, int>(
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
            new StoredProperty<Texture2DDefinition, Texture2DFilterMode>(
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
            new StoredProperty<Texture2DDefinition, Texture2DAddressMode>(
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
            new StoredProperty<Texture2DDefinition, SpriteEffects>(
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

        public readonly static IProperty BlendModeProperty =
            new StoredProperty<Texture2DDefinition, Texture2DBlendMode>(
                nameof(BlendModeProperty),
                defaultValue: Texture2DBlendMode.AlphaBlend,
                category: Observables.PropertyCategories.Rendering,
                metadata: new PropertySerializationMetadata(
                    deserialize: (p, r) => r.ReadEnum<Texture2DBlendMode>(context: p.Name),
                    serialize: (p, w, v) => w.WriteEnum<Texture2DBlendMode>(context: p.Name, (Texture2DBlendMode)(v ?? Texture2DBlendMode.AlphaBlend)))
                {
                    Visibility = PropertyVisibility.Visible
                });

        public Texture2DBlendMode BlendMode
        {
            get => GetValue<Texture2DBlendMode>(BlendModeProperty);
            set => SetValue<Texture2DBlendMode>(BlendModeProperty, value);
        }

        public readonly static IProperty UseMipmapsProperty =
            new StoredProperty<Texture2DDefinition, bool>(
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
    }
}
