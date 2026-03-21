using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Visuals.Regions
{
    /// <summary>
    /// Provides extension methods to create <see cref="TextureRegion"/> and patch regions from texture atlases.
    /// </summary>
    public static class TextureAtlasExtensions
    {
        /// <summary>
        /// Creates a <see cref="TextureRegion"/> from a regular <see cref="TextureAtlas"/> entry.
        /// </summary>
        /// <param name="atlas">The texture atlas containing the source regions.</param>
        /// <param name="key">The string key of the region within the atlas.</param>
        /// <returns>A new <see cref="TextureRegion"/> pointing to the specified sub-region.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="atlas"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> does not exist in the atlas.</exception>
        public static TextureRegion ToRegion(this TextureAtlas atlas, string key, RegionOptions? options = null)
        {
            if (atlas == null)
                throw new ArgumentNullException(nameof(atlas));

            var region = atlas[key]; // May throw KeyNotFoundException if key is missing
            return new TextureRegion(atlas.Texture, region.SourceRectangle)
            {
                Options = options
            };
        }

        public static RegionBrush ToBrush(this TextureAtlas atlas, string key, RegionOptions? options = null)
        {
            return new RegionBrush(ToRegion(atlas, key, options));
        }

        /// <summary>
        /// Creates a <see cref="TextureRegion"/> from an <see cref="IndexedTextureAtlas{TEnum}"/> entry.
        /// </summary>
        /// <typeparam name="TEnum">An enum type used as atlas keys.</typeparam>
        /// <param name="atlas">The indexed texture atlas containing the source regions.</param>
        /// <param name="key">The enum key of the region within the atlas.</param>
        /// <returns>A new <see cref="TextureRegion"/> pointing to the specified sub-region.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="atlas"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> does not exist in the atlas.</exception>
        public static TextureRegion ToRegion<TEnum>(this IndexedTextureAtlas<TEnum> atlas, TEnum key, RegionOptions? options = null)
            where TEnum : struct, Enum
        {
            if (atlas == null)
                throw new ArgumentNullException(nameof(atlas));

            var region = atlas[key]; // May throw KeyNotFoundException if key is missing
            return new TextureRegion(atlas.Texture, region.SourceRectangle)
            {
                Options = options
            };
        }

        public static RegionBrush ToBrush<TEnum>(this IndexedTextureAtlas<TEnum> atlas, TEnum key, RegionOptions? options = null)
            where TEnum : struct, Enum
        {
            return new RegionBrush(ToRegion<TEnum>(atlas, key, options));
        }

        /// <summary>
        /// Creates a <see cref="ThreePatchRegion"/> from an <see cref="IndexedTextureAtlas{TEnum}"/>,
        /// using three specified keys to define the fixed and stretchable segments.
        /// </summary>
        /// <typeparam name="TEnum">The enum type used as keys in the texture atlas.</typeparam>
        /// <param name="atlas">The indexed texture atlas to retrieve regions from.</param>
        /// <param name="firstKey">The key identifying the fixed first segment (left or top).</param>
        /// <param name="middleKey">The key identifying the stretchable middle segment.</param>
        /// <param name="lastKey">The key identifying the fixed last segment (right or bottom).</param>
        /// <param name="orientation">The orientation along which the middle segment will be stretched (horizontal or vertical).</param>
        /// <returns>A new <see cref="ThreePatchRegion"/> constructed from the specified regions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="atlas"/> is <c>null</c>.</exception>
        public static ThreePatchRegion ToThreePatchRegion<TEnum>(
            this IndexedTextureAtlas<TEnum> atlas,
            TEnum firstKey,
            TEnum middleKey,
            TEnum lastKey,
            Orientation orientation,
            RegionOptions? options = null)
            where TEnum : struct, Enum
        {
            if (atlas == null)
                throw new ArgumentNullException(nameof(atlas));

            var firstRegion = atlas.ToRegion(firstKey);
            var middleRegion = atlas.ToRegion(middleKey);
            var lastRegion = atlas.ToRegion(lastKey);

            return new ThreePatchRegion(firstRegion, middleRegion, lastRegion, orientation)
            {
                Options = options
            };
        }

        public static RegionBrush ToThreePatchBrush<TEnum>(
            this IndexedTextureAtlas<TEnum> atlas,
            TEnum firstKey,
            TEnum middleKey,
            TEnum lastKey,
            Orientation orientation,
            RegionOptions? options = null)
            where TEnum : struct, Enum
        {
            return new RegionBrush(ToThreePatchRegion<TEnum>(atlas, firstKey, middleKey, lastKey, orientation, options));
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
        /// Useful for scrolling or animating the pattern.
        /// </param>
        /// <returns>A <see cref="TiledRegion"/> wrapping the atlas region.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="atlas"/> is <c>null</c>.</exception>
        public static TiledRegion ToTiledRegion<TEnum>(
            this IndexedTextureAtlas<TEnum> atlas,
            TEnum key,
            TileMode mode = TileMode.TileXY,
            Point? offset = null,
            RegionOptions? options = null)
            where TEnum : struct, Enum
        {
            if (atlas == null)
                throw new ArgumentNullException(nameof(atlas));

            var region = atlas.ToRegion(key);
            return new TiledRegion(region, mode, offset)
            {
                Options = options
            };
        }

        public static RegionBrush ToTiledBrush<TEnum>(
            this IndexedTextureAtlas<TEnum> atlas,
            TEnum key,
            TileMode mode = TileMode.TileXY,
            Point? offset = null,
            RegionOptions? options = null)
            where TEnum : struct, Enum
        {
            return new RegionBrush(ToTiledRegion<TEnum>(atlas, key, mode, offset, options));
        }

        /// <summary>
        /// Creates a <see cref="NinePatchRegionOld"/> from an <see cref="IndexedTextureAtlas{TEnum}"/>,
        /// using nine specified keys to define the nine segments.
        /// </summary>
        /// <typeparam name="TEnum">The enum type used as keys in the texture atlas.</typeparam>
        /// <param name="atlas">The indexed texture atlas to retrieve regions from.</param>
        /// <param name="topLeftKey">The key for the top-left segment.</param>
        /// <param name="topCenterKey">The key for the top-center segment.</param>
        /// <param name="topRightKey">The key for the top-right segment.</param>
        /// <param name="middleLeftKey">The key for the middle-left segment.</param>
        /// <param name="centerKey">The key for the center segment.</param>
        /// <param name="middleRightKey">The key for the middle-right segment.</param>
        /// <param name="bottomLeftKey">The key for the bottom-left segment.</param>
        /// <param name="bottomCenterKey">The key for the bottom-center segment.</param>
        /// <param name="bottomRightKey">The key for the bottom-right segment.</param>
        /// <returns>A new <see cref="NinePatchRegionOld"/> constructed from the specified regions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="atlas"/> is <c>null</c>.</exception>
        public static NinePatchRegion ToNinePatchRegion<TEnum>(
            this IndexedTextureAtlas<TEnum> atlas,
            TEnum topLeftKey,
            TEnum topCenterKey,
            TEnum topRightKey,
            TEnum middleLeftKey,
            TEnum centerKey,
            TEnum middleRightKey,
            TEnum bottomLeftKey,
            TEnum bottomCenterKey,
            TEnum bottomRightKey,
            RegionOptions? options = null)
            where TEnum : struct, Enum
        {
            if (atlas == null)
                throw new ArgumentNullException(nameof(atlas));

            var topLeft = atlas.ToRegion(topLeftKey);
            var topCenter = atlas.ToRegion(topCenterKey);
            var topRight = atlas.ToRegion(topRightKey);

            var middleLeft = atlas.ToRegion(middleLeftKey);
            var center = atlas.ToRegion(centerKey);
            var middleRight = atlas.ToRegion(middleRightKey);

            var bottomLeft = atlas.ToRegion(bottomLeftKey);
            var bottomCenter = atlas.ToRegion(bottomCenterKey);
            var bottomRight = atlas.ToRegion(bottomRightKey);

            return new NinePatchRegion(
                topLeft,
                topCenter,
                topRight,
                middleLeft,
                center,
                middleRight,
                bottomLeft,
                bottomCenter,
                bottomRight)
            {
                Options = options
            };
        }

        public static RegionBrush ToNinePatchBrush<TEnum>(
            this IndexedTextureAtlas<TEnum> atlas,
            TEnum topLeftKey,
            TEnum topCenterKey,
            TEnum topRightKey,
            TEnum middleLeftKey,
            TEnum centerKey,
            TEnum middleRightKey,
            TEnum bottomLeftKey,
            TEnum bottomCenterKey,
            TEnum bottomRightKey,
            RegionOptions? options = null)
            where TEnum : struct, Enum
        {
            return new RegionBrush(ToNinePatchRegion<TEnum>(
                atlas, 
                topLeftKey, 
                topCenterKey, 
                topRightKey,
                middleLeftKey,
                centerKey,
                middleRightKey,
                bottomLeftKey,
                bottomCenterKey,
                bottomRightKey,
                options
            ));
        }
    }
}
