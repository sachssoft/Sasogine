using Microsoft.Xna.Framework.Audio;
using Sachssoft.Observables;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Represents an audio asset (WAV) that can be loaded from an <see cref="IAssetSource"/>.
    /// </summary>
    public class SoundAsset : AssetBase<SoundEffect>
    {
        /// <summary>
        /// Builds a <see cref="SoundEffect"/> from the specified WAV stream.
        /// </summary>
        /// <param name="stream">The stream containing WAV data.</param>
        /// <returns>The loaded <see cref="SoundEffect"/> instance, or <c>null</c> if loading fails.</returns>
        protected override SoundEffect? Build(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                return null;

            try
            {
                return SoundEffect.FromStream(stream);
            }
            catch (Exception ex)
            {
                Exception = ex; 
                return null;
            }
        }


        public static readonly IProperty VolumeProperty =
            new StoredProperty<SoundAsset, float>(
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
            new StoredProperty<SoundAsset, bool>(
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
            new StoredProperty<SoundAsset, float>(
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
