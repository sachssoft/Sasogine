//using Microsoft.Xna.Framework;
//using Sachssoft.Sasogine.Assets.Graphics;
//using Sachssoft.Sasogine.Common;
//using Sachssoft.Sasogine.Resources;
//using System;

//namespace Sachssoft.Sasogine.Resources.Markup
//{
//    /// <summary>
//    /// Provides extension methods for converting loaded frame set data
//    /// into frame set collections.
//    /// </summary>
//    public static class FrameSetLoaderExtensions
//    {
//        /// <summary>
//        /// Loads the frame set and converts its entries into an indexed frame set.
//        /// </summary>
//        /// <typeparam name="TEnum">
//        /// The enumeration type used as the frame set index.
//        /// </typeparam>
//        /// <param name="frameSetLoader">
//        /// The frame set loader.
//        /// </param>
//        /// <param name="asset">
//        /// The texture asset containing the frames.
//        /// </param>
//        /// <param name="convert">
//        /// An optional function used to convert frame names into enumeration values.
//        /// When <see langword="null"/>, frame names are parsed directly as enumeration values.
//        /// </param>
//        /// <returns>
//        /// An indexed frame set containing the loaded frames.
//        /// </returns>
//        /// <exception cref="ArgumentNullException">
//        /// Thrown when <paramref name="frameSetLoader"/> or <paramref name="asset"/>
//        /// is <see langword="null"/>.
//        /// </exception>
//        /// <exception cref="InvalidOperationException">
//        /// Thrown when a frame name cannot be converted to <typeparamref name="TEnum"/>
//        /// and no conversion function was provided.
//        /// </exception>
//        public static IndexedFrameSet<TEnum> ToIndexed<TEnum>(
//            this FrameSetLoader frameSetLoader,
//            Texture2DAsset asset,
//            Func<string, TEnum>? convert = null)
//            where TEnum : struct, Enum
//        {
//            ArgumentNullException.ThrowIfNull(frameSetLoader);
//            ArgumentNullException.ThrowIfNull(asset);

//            var frameSet = new IndexedFrameSet<TEnum>(asset);

//            foreach (var entry in frameSetLoader.Load().Entries)
//            {
//                TEnum index;

//                if (convert is not null)
//                {
//                    index = convert(entry.Name);
//                }
//                else if (!Enum.TryParse(
//                    entry.Name,
//                    out index))
//                {
//                    throw new InvalidOperationException(
//                        $"The frame '{entry.Name}' could not be converted to {typeof(TEnum).Name}.");
//                }

//                frameSet.Add(
//                    index,
//                    new Point(entry.X, entry.Y),
//                    new PixelSize2(entry.Width, entry.Height));
//            }

//            return frameSet;
//        }

//        /// <summary>
//        /// Loads the frame set and converts its entries into a keyed frame set.
//        /// </summary>
//        /// <param name="frameSetLoader">
//        /// The frame set loader.
//        /// </param>
//        /// <param name="asset">
//        /// The texture asset containing the frames.
//        /// </param>
//        /// <param name="convert">
//        /// An optional function used to convert frame names into keys.
//        /// When <see langword="null"/>, the original frame names are used.
//        /// </param>
//        /// <returns>
//        /// A keyed frame set containing the loaded frames.
//        /// </returns>
//        /// <exception cref="ArgumentNullException">
//        /// Thrown when <paramref name="frameSetLoader"/> or <paramref name="asset"/>
//        /// is <see langword="null"/>.
//        /// </exception>
//        public static KeyedFrameSet ToKeyed(
//            this FrameSetLoader frameSetLoader,
//            Texture2DAsset asset,
//            Func<string, string>? convert = null)
//        {
//            ArgumentNullException.ThrowIfNull(frameSetLoader);
//            ArgumentNullException.ThrowIfNull(asset);

//            var frameSet = new KeyedFrameSet(asset);

//            foreach (var entry in frameSetLoader.Load().Entries)
//            {
//                var key = convert?.Invoke(entry.Name) ?? entry.Name;

//                frameSet.Add(
//                    key,
//                    new Point(entry.X, entry.Y),
//                    new PixelSize2(entry.Width, entry.Height));
//            }

//            return frameSet;
//        }
//    }
//}