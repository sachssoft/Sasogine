using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Engine.Resources.Importers;

/// <summary>Converts imported entries into frame sets while preserving their atlas page indices.</summary>
public static class FrameSetImporterExtensions
{
    /// <summary>Imports enum-keyed frames from one atlas texture.</summary>
    /// <typeparam name="TEnum">The frame key enum.</typeparam>
    /// <param name="frameSetImporter">The document importer.</param>
    /// <param name="texture">The borrowed atlas texture.</param>
    /// <param name="convert">An optional name converter; otherwise enum names are parsed.</param>
    /// <returns>The imported enum-indexed frame set.</returns>
    public static IndexedFrameSet<TEnum> ToIndexed<TEnum>(this FrameSetImporter frameSetImporter,
        Texture2D texture, Func<string, TEnum>? convert = null) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(texture);
        return frameSetImporter.ToIndexed(new[] { texture }, convert);
    }

    /// <summary>Imports enum-keyed frames from ordered atlas textures.</summary>
    /// <typeparam name="TEnum">The frame key enum.</typeparam>
    /// <param name="frameSetImporter">The document importer.</param>
    /// <param name="textures">The borrowed textures in atlas page order.</param>
    /// <param name="convert">An optional name converter; otherwise enum names are parsed.</param>
    /// <returns>The imported enum-indexed frame set.</returns>
    public static IndexedFrameSet<TEnum> ToIndexed<TEnum>(this FrameSetImporter frameSetImporter,
        IReadOnlyList<Texture2D> textures, Func<string, TEnum>? convert = null) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(frameSetImporter);
        return frameSetImporter.Import().ToIndexed(textures, convert);
    }

    /// <summary>Converts imported entries into an enum-keyed collection without discarding page indices.</summary>
    /// <typeparam name="TEnum">The frame key enum.</typeparam>
    /// <param name="entries">The imported frame entries.</param>
    /// <param name="textures">The borrowed textures in atlas page order.</param>
    /// <param name="convert">An optional name converter; otherwise enum names are parsed.</param>
    /// <returns>The enum-indexed collection.</returns>
    public static IndexedFrameSet<TEnum> ToIndexed<TEnum>(this IEnumerable<FrameSetEntry> entries,
        IReadOnlyList<Texture2D> textures, Func<string, TEnum>? convert = null) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(entries);
        var result = new IndexedFrameSet<TEnum>(textures);
        foreach (FrameSetEntry entry in entries)
        {
            TEnum index;
            if (convert is not null) index = convert(entry.Name);
            else if (!Enum.TryParse(entry.Name, out index))
                throw new InvalidOperationException($"The frame '{entry.Name}' could not be converted to {typeof(TEnum).Name}.");
            result.Add(index, new PixelPoint2(entry.X, entry.Y), new PixelSize2(entry.Width, entry.Height), entry.TextureIndex);
        }
        return result;
    }

    /// <summary>Converts imported entries into an integer-indexed collection with optional name mapping.</summary>
    /// <param name="entries">The imported frame entries.</param>
    /// <param name="textures">The borrowed textures in atlas page order.</param>
    /// <param name="convert">An optional index converter; otherwise indices follow insertion order.</param>
    /// <returns>The integer-indexed collection, including all page associations.</returns>
    public static IndexedFrameSet ToIndexed(this IEnumerable<FrameSetEntry> entries,
        IReadOnlyList<Texture2D> textures, Func<string, int>? convert = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var result = new IndexedFrameSet(textures);
        int index = 0;
        foreach (FrameSetEntry entry in entries)
        {
            result.Add(convert?.Invoke(entry.Name) ?? index,
                new PixelPoint2(entry.X, entry.Y), new PixelSize2(entry.Width, entry.Height), entry.TextureIndex);
            index++;
        }
        return result;
    }

    /// <summary>Imports string-keyed frames from one atlas texture.</summary>
    /// <param name="frameSetImporter">The document importer.</param>
    /// <param name="texture">The borrowed atlas texture.</param>
    /// <param name="convert">An optional frame-name converter.</param>
    /// <returns>The imported keyed frame set.</returns>
    public static KeyedFrameSet ToKeyed(this FrameSetImporter frameSetImporter,
        Texture2D texture, Func<string, string>? convert = null)
    {
        ArgumentNullException.ThrowIfNull(texture);
        return frameSetImporter.ToKeyed(new[] { texture }, convert);
    }

    /// <summary>Imports string-keyed frames from ordered atlas textures.</summary>
    /// <param name="frameSetImporter">The document importer.</param>
    /// <param name="textures">The borrowed textures in atlas page order.</param>
    /// <param name="convert">An optional frame-name converter.</param>
    /// <returns>The imported keyed frame set.</returns>
    public static KeyedFrameSet ToKeyed(this FrameSetImporter frameSetImporter,
        IReadOnlyList<Texture2D> textures, Func<string, string>? convert = null)
    {
        ArgumentNullException.ThrowIfNull(frameSetImporter);
        return frameSetImporter.Import().ToKeyed(textures, convert);
    }

    /// <summary>Converts imported entries into a keyed collection without discarding page indices.</summary>
    /// <param name="entries">The imported entries.</param>
    /// <param name="textures">The borrowed textures in atlas page order.</param>
    /// <param name="convert">An optional frame-name converter.</param>
    /// <returns>The imported keyed collection.</returns>
    public static KeyedFrameSet ToKeyed(this IEnumerable<FrameSetEntry> entries,
        IReadOnlyList<Texture2D> textures, Func<string, string>? convert = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var result = new KeyedFrameSet(textures);
        foreach (FrameSetEntry entry in entries)
            result.Add(convert?.Invoke(entry.Name) ?? entry.Name,
                new PixelPoint2(entry.X, entry.Y), new PixelSize2(entry.Width, entry.Height), entry.TextureIndex);
        return result;
    }

    /// <summary>Combines separate single-page documents in texture order, including empty pages.</summary>
    /// <param name="pages">The importers ordered like the texture collection passed to conversion.</param>
    /// <returns>Entries with the document's zero-based position as their texture index.</returns>
    /// <exception cref="InvalidDataException">A document already references a nonzero page.</exception>
    public static IEnumerable<FrameSetEntry> ImportPages(this IReadOnlyList<FrameSetImporter> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);
        for (int i = 0; i < pages.Count; i++)
        {
            FrameSetImporter importer = pages[i] ?? throw new ArgumentException("Page importers must not contain null.", nameof(pages));
            foreach (FrameSetEntry entry in importer.Import())
            {
                if (entry.TextureIndex != 0)
                    throw new InvalidDataException("ImportPages expects one single-page document per texture.");
                entry.TextureIndex = i;
                yield return entry;
            }
        }
    }
}