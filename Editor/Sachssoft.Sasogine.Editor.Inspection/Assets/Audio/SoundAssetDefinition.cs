using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Assets.Audio;

namespace Sachssoft.Sasogine.Inspection.Assets.Audio
{
    public class SoundAssetDefinition : AssetDefinition, ISoundAssetDefinition, ITypeRegistry
    {
        static void ITypeRegistry.RegisterProperties(TypeRegistryContext context)
        {
            context.RegisterProperty(VolumeProperty);
            context.RegisterProperty(LoopProperty);
            context.RegisterProperty(PitchProperty);
        }

        public SoundAssetDefinition()
        {
        }


        public static readonly IProperty VolumeProperty =
            new StoredProperty<SoundAssetDefinition, float>(
                nameof(Volume),
                defaultValue: 1.0f,
                category: PropertyCategories.General);

        /// <summary>Standardlautstärke beim Abspielen (0.0 = stumm, 1.0 = volle Lautstärke)</summary>
        public float Volume
        {
            get => GetValue<float>(VolumeProperty);
            set => SetValue<float>(VolumeProperty, value);
        }


        public static readonly IProperty LoopProperty =
            new StoredProperty<SoundAssetDefinition, bool>(
                nameof(Loop),
                defaultValue: false,
                category: PropertyCategories.General);
        /// <summary>Gibt an, ob der Sound standardmäßig geloopt werden soll</summary>
        public bool Loop
        {
            get => GetValue<bool>(LoopProperty);
            set => SetValue<bool>(LoopProperty, value);
        }


        public static readonly IProperty PitchProperty =
            new StoredProperty<SoundAssetDefinition, float>(
                nameof(Pitch),
                defaultValue: 0.0f,
                category: PropertyCategories.General);
        /// <summary>Tonhöhenanpassung (-1.0 bis 1.0, 0 = normal)</summary>
        /// 
        public float Pitch
        {
            get => GetValue<float>(PitchProperty);
            set => SetValue<float>(PitchProperty, value);
        }
    }
}
