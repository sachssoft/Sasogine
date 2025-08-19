using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources
{
    /// <summary>
    /// Represents a texture atlas with frames indexed by an enum key.
    /// </summary>
    /// <typeparam name="TEnum">An enum type used as the key for frames.</typeparam>
    public class IndexedTextureAtlas<TEnum> : TextureAtlas where TEnum : struct, Enum
    {
        private readonly Dictionary<TEnum, TextureAtlasFrame> _enumFrames = new();

        /// <summary>
        /// Adds a frame to the atlas indexed by the specified enum key.
        /// </summary>
        /// <param name="key">The enum key to associate with the frame.</param>
        /// <param name="frameName">The name of the frame in the base TextureAtlas.</param>
        /// <exception cref="InvalidOperationException">Thrown if the frameName is not found in the base TextureAtlas.</exception>
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
        /// Gets the frame associated with the specified enum key.
        /// </summary>
        /// <param name="key">The enum key.</param>
        /// <returns>The <see cref="TextureAtlasFrame"/> associated with the key.</returns>
        public TextureAtlasFrame this[TEnum key] => _enumFrames[key];
    }
}
