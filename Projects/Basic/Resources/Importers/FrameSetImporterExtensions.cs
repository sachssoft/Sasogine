using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Common;
using System;

namespace Sachssoft.Engine.Resources.Importers;

/// <summary>
/// Provides extension methods for converting imported frame set data
/// into frame set collections.
/// </summary>
public static class FrameSetImporterExtensions
{
    /// <summary>
    /// Imports the frame set and converts its entries into an indexed frame set.
    /// </summary>
    /// <typeparam name="TEnum">
    /// The enumeration type used as the frame set index.
    /// </typeparam>
    /// <param name="frameSetImporter">
    /// The frame set importer.
    /// </param>
    /// <param name="texture">
    /// The texture containing the frames.
    /// </param>
    /// <param name="convert">
    /// An optional function used to convert frame names into enumeration values.
    /// When <see langword="null"/>, frame names are parsed directly as enumeration values.
    /// </param>
    /// <returns>
    /// An indexed frame set containing the imported frames.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="frameSetImporter"/> or <paramref name="texture"/>
    /// is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a frame name cannot be converted to <typeparamref name="TEnum"/>
    /// and no conversion function was provided.
    /// </exception>
    public static IndexedFrameSet<TEnum> ToIndexed<TEnum>(
        this FrameSetImporter frameSetImporter,
        Texture2D texture,
        Func<string, TEnum>? convert = null)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(frameSetImporter);
        ArgumentNullException.ThrowIfNull(texture);

        var frameSet = new IndexedFrameSet<TEnum>(texture);

        foreach (var entry in frameSetImporter.Import())
        {
            TEnum index;

            if (convert is not null)
            {
                index = convert(entry.Name);
            }
            else if (!Enum.TryParse(
                entry.Name,
                out index))
            {
                throw new InvalidOperationException(
                    $"The frame '{entry.Name}' could not be converted to {typeof(TEnum).Name}.");
            }

            frameSet.Add(
                index,
                new Point(entry.X, entry.Y),
                new PixelSize2(entry.Width, entry.Height));
        }

        return frameSet;
    }

    /// <summary>
    /// Imports the frame set and converts its entries into a keyed frame set.
    /// </summary>
    /// <param name="frameSetImporter">
    /// The frame set importer.
    /// </param>
    /// <param name="texture">
    /// The texture containing the frames.
    /// </param>
    /// <param name="convert">
    /// An optional function used to convert frame names into keys.
    /// When <see langword="null"/>, the original frame names are used.
    /// </param>
    /// <returns>
    /// A keyed frame set containing the imported frames.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="frameSetImporter"/> or <paramref name="texture"/>
    /// is <see langword="null"/>.
    /// </exception>
    public static KeyedFrameSet ToKeyed(
        this FrameSetImporter frameSetImporter,
        Texture2D texture,
        Func<string, string>? convert = null)
    {
        ArgumentNullException.ThrowIfNull(frameSetImporter);
        ArgumentNullException.ThrowIfNull(texture);

        var frameSet = new KeyedFrameSet(texture);

        foreach (var entry in frameSetImporter.Import())
        {
            var key = convert?.Invoke(entry.Name) ?? entry.Name;

            frameSet.Add(
                key,
                new Point(entry.X, entry.Y),
                new PixelSize2(entry.Width, entry.Height));
        }

        return frameSet;
    }
}