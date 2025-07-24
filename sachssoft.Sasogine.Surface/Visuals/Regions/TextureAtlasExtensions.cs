using Microsoft.Xna.Framework;
using sachssoft.Sasogine.Resources;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using System;

namespace sachssoft.Sasogine.Surface.Visuals.Regions
{
    /// <summary>
    /// Provides extension methods to create <see cref="TextureRegion"/> instances from texture atlases.
    /// </summary>
    public static class TextureAtlasExtensions
    {
        /// <summary>
        /// Creates a <see cref="TextureRegion"/> from a regular <see cref="TextureAtlas"/> entry.
        /// </summary>
        /// <param name="atlas">The texture atlas containing the source regions.</param>
        /// <param name="key">The string key of the region within the atlas.</param>
        /// <returns>A new <see cref="TextureRegion"/> pointing to the specified sub-region.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="atlas"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> does not exist in the atlas.</exception>
        public static TextureRegion ToRegion(this TextureAtlas atlas, string key)
        {
            if (atlas == null)
                throw new ArgumentNullException(nameof(atlas));

            var region = atlas[key]; // throws KeyNotFoundException if missing
            return new TextureRegion(atlas.Texture, region.SourceRectangle);
        }

        /// <summary>
        /// Creates a <see cref="TextureRegion"/> from an <see cref="IndexedTextureAtlas{TEnum}"/> entry.
        /// </summary>
        /// <typeparam name="TEnum">An enum type used as atlas keys.</typeparam>
        /// <param name="atlas">The indexed texture atlas containing the source regions.</param>
        /// <param name="key">The enum key of the region within the atlas.</param>
        /// <returns>A new <see cref="TextureRegion"/> pointing to the specified sub-region.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="atlas"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> does not exist in the atlas.</exception>
        public static TextureRegion ToRegion<TEnum>(this IndexedTextureAtlas<TEnum> atlas, TEnum key)
            where TEnum : struct, Enum
        {
            if (atlas == null)
                throw new ArgumentNullException(nameof(atlas));

            var region = atlas[key]; // throws KeyNotFoundException if missing
            return new TextureRegion(atlas.Texture, region.SourceRectangle);
        }

        /// <summary>
        /// Creates a <see cref="ThreePatchRegion"/> from an <see cref="IndexedTextureAtlas{TEnum}"/>,
        /// using three specified keys to define the fixed and stretchable segments.
        /// </summary>
        /// <typeparam name="TEnum">The enum type used as keys in the texture atlas.</typeparam>
        /// <param name="atlas">The indexed texture atlas to retrieve regions from.</param>
        /// <param name="first_key">The key identifying the fixed first segment (left or top).</param>
        /// <param name="middle_key">The key identifying the stretchable middle segment.</param>
        /// <param name="last_key">The key identifying the fixed last segment (right or bottom).</param>
        /// <param name="orientation">The orientation along which the middle segment will be stretched (horizontal or vertical).</param>
        /// <returns>A new <see cref="ThreePatchRegion"/> constructed from the specified regions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="atlas"/> is null.</exception>
        public static ThreePatchRegion ToThreePatchRegion<TEnum>(this IndexedTextureAtlas<TEnum> atlas, TEnum first_key, TEnum middle_key, TEnum last_key, Orientation orientation)
             where TEnum : struct, Enum
        {
            if (atlas == null)
                throw new ArgumentNullException(nameof(atlas));

            var first_region = atlas.ToRegion(first_key);
            var middle_region = atlas.ToRegion(middle_key);
            var last_region = atlas.ToRegion(last_key);

            return new ThreePatchRegion(first_region, middle_region, last_region, orientation);
        }

        /// <summary>
        /// Creates a <see cref="TiledRegion"/> from the specified texture atlas entry.
        /// </summary>
        /// <typeparam name="TEnum">Enum type used as key in the atlas.</typeparam>
        /// <param name="atlas">The indexed texture atlas.</param>
        /// <param name="key">The enum key that identifies the texture region.</param>
        /// <param name="mode">How the texture is tiled (default XY).</param>
        /// <param name="offset">
        /// Optional offset (in pixels) applied to the tile pattern.
        /// Useful for scrolling/animating the pattern.
        /// </param>
        /// <returns>A <see cref="TiledRegion"/> wrapping the atlas region.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="atlas"/> is null.</exception>
        public static TiledRegion ToTiledRegion<TEnum>(
            this IndexedTextureAtlas<TEnum> atlas,
            TEnum key,
            TileMode mode = TileMode.XY,
            Point? offset = null)
            where TEnum : struct, Enum
        {
            if (atlas == null)
                throw new ArgumentNullException(nameof(atlas));

            var region = atlas.ToRegion(key);
            return new TiledRegion(region, mode, offset);
        }
    }
}
