using Microsoft.Xna.Framework.Audio;
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
    }
}
