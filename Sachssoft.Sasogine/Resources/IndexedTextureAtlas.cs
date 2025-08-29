using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Resources
{
    /// <summary>
    /// Represents a texture atlas with frames that can be indexed using an enum type.
    /// Provides methods to add, retrieve, and crop frames based on the enum keys.
    /// </summary>
    /// <typeparam name="TEnum">The enum type used as the key for frames.</typeparam>
    public class IndexedTextureAtlas<TEnum> : TextureAtlas where TEnum : struct, Enum
    {
        /// <summary>
        /// Internal dictionary storing the mapping from enum keys to texture frames.
        /// </summary>
        private readonly Dictionary<TEnum, TextureAtlasFrame> _enumFrames = new();

        /// <summary>
        /// Adds a frame from the base <see cref="TextureAtlas"/> and associates it with the specified enum key.
        /// </summary>
        /// <param name="key">The enum key to associate with the frame.</param>
        /// <param name="frameName">The name of the frame in the base <see cref="TextureAtlas"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if the frameName is not found in the base <see cref="TextureAtlas"/>.</exception>
        public void AddFrame(TEnum key, string frameName)
        {
            if (Frames.TryGetValue(frameName, out var frame))
            {
                _enumFrames[key] = frame;
                return;
            }

            throw new InvalidOperationException($"Frame '{frameName}' not found in base TextureAtlas.");
        }

        /// <summary>
        /// Gets the <see cref="TextureAtlasFrame"/> associated with the specified enum key.
        /// </summary>
        /// <param name="key">The enum key to retrieve the frame for.</param>
        /// <returns>The <see cref="TextureAtlasFrame"/> associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key does not exist in the atlas.</exception>
        public TextureAtlasFrame this[TEnum key] => _enumFrames[key];

        /// <summary>
        /// Crops a texture from the atlas using the frame associated with the specified enum key.
        /// </summary>
        /// <param name="key">The enum key identifying the frame to crop.</param>
        /// <returns>A <see cref="Texture2D"/> representing the cropped texture.</returns>
        public Texture2D Crop(TEnum key)
        {
            return Crop(_enumFrames[key].Name);
        }

        /// <summary>
        /// Tries to crop a texture from the atlas using the frame associated with the specified enum key.
        /// </summary>
        /// <param name="key">The enum key identifying the frame to crop.</param>
        /// <param name="croppedTexture">
        /// When this method returns, contains the cropped <see cref="Texture2D"/> if successful; otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if the cropping succeeded; otherwise, <c>false</c>.</returns>
        public bool TryCrop(TEnum key, [MaybeNullWhen(false)] out Texture2D croppedTexture)
        {
            return TryCrop(_enumFrames[key].Name, out croppedTexture);
        }
    }
}
