using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using System;

namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Provides extension methods for reading, modifying, copying, cropping,
/// and processing two-dimensional textures.
/// </summary>
public static class Texture2DExtensions
{
    /// <summary>
    /// Reads all pixels from the specified texture.
    /// </summary>
    /// <param name="texture">The texture whose pixel data is read.</param>
    /// <returns>An array containing the texture pixels in row-major order.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static Color[] GetPixels(this Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var data = new Color[texture.Width * texture.Height];
        texture.GetData(data);

        return data;
    }

    /// <summary>
    /// Replaces all pixels of the specified texture.
    /// </summary>
    /// <param name="texture">The texture whose pixel data is replaced.</param>
    /// <param name="data">
    /// The pixel data to assign. The number of pixels must match the
    /// dimensions of the texture.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="texture"/> or <paramref name="data"/>
    /// is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of pixels in <paramref name="data"/> does not
    /// match the texture dimensions.
    /// </exception>
    public static void SetPixels(this Texture2D texture, Color[] data)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length != texture.Width * texture.Height)
            throw new ArgumentException("Pixel array size does not match texture dimensions.");

        texture.SetData(data);
    }

    /// <summary>
    /// Creates an independent copy of the specified texture and its pixel data.
    /// </summary>
    /// <param name="texture">The texture to clone.</param>
    /// <returns>
    /// A new texture containing the same pixel data, dimensions, and surface format.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D Clone(this Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var data = texture.GetPixels();

        var copy = new Texture2D(
            texture.GraphicsDevice,
            texture.Width,
            texture.Height,
            false,
            texture.Format);

        copy.SetData(data);

        return copy;
    }

    /// <summary>
    /// Creates a texture containing the specified rectangular region
    /// of the source texture.
    /// </summary>
    /// <param name="texture">The source texture to crop.</param>
    /// <param name="sourceRect">The rectangular region to extract.</param>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the resulting texture.
    /// </param>
    /// <returns>
    /// A new texture containing the portion of the source texture that
    /// intersects <paramref name="sourceRect"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="texture"/> or <paramref name="graphicsDevice"/>
    /// is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sourceRect"/> has a non-positive size
    /// or does not intersect the source texture.
    /// </exception>
    public static Texture2D Crop(
        this Texture2D texture,
        Rectangle sourceRect,
        GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        if (sourceRect.Width <= 0 || sourceRect.Height <= 0)
            throw new ArgumentException("Crop rectangle must have positive size.");

        var bounds = new Rectangle(0, 0, texture.Width, texture.Height);

        if (!bounds.Intersects(sourceRect))
            throw new ArgumentException("Crop rectangle is outside texture bounds.");

        sourceRect = Rectangle.Intersect(bounds, sourceRect);

        var data = new Color[sourceRect.Width * sourceRect.Height];
        texture.GetData(0, sourceRect, data, 0, data.Length);

        var result = new Texture2D(
            graphicsDevice,
            sourceRect.Width,
            sourceRect.Height,
            false,
            texture.Format);

        result.SetData(data);

        return result;
    }

    /// <summary>
    /// Creates a texture containing the specified rectangular region
    /// of the source texture.
    /// </summary>
    /// <param name="texture">The source texture to crop.</param>
    /// <param name="sourceRect">The rectangular region to extract.</param>
    /// <returns>
    /// A new texture containing the portion of the source texture that
    /// intersects <paramref name="sourceRect"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sourceRect"/> has a non-positive size
    /// or does not intersect the source texture.
    /// </exception>
    public static Texture2D Crop(this Texture2D texture, Rectangle sourceRect)
        => Crop(texture, sourceRect, texture.GraphicsDevice);

    /// <summary>
    /// Extracts a single cell from a grid-based texture or sprite sheet.
    /// </summary>
    /// <param name="texture">The source texture containing the cells.</param>
    /// <param name="cell">The zero-based column and row identifying the cell to extract.</param>
    /// <param name="cellSize">The size of each cell in pixels.</param>
    /// <param name="graphicsDevice">The graphics device used to create the resulting texture.</param>
    /// <returns>A new texture containing the selected cell.</returns>
    public static Texture2D TileCrop(
        this Texture2D texture,
        Point cell,
        PixelSize2 cellSize,
        GraphicsDevice graphicsDevice)
    {
        var rect = new Rectangle(
            cell.X * cellSize.Width,
            cell.Y * cellSize.Height,
            cellSize.Width,
            cellSize.Height);

        return Crop(texture, rect, graphicsDevice);
    }

    /// <summary>
    /// Extracts a single cell from a grid-based texture or sprite sheet.
    /// </summary>
    /// <param name="texture">The source texture containing the cells.</param>
    /// <param name="cell">The zero-based column and row identifying the cell to extract.</param>
    /// <param name="cellSize">The size of each cell in pixels.</param>
    /// <returns>A new texture containing the selected cell.</returns>
    public static Texture2D TileCrop(this Texture2D texture, Point cell, PixelSize2 cellSize)
        => TileCrop(texture, cell, cellSize, texture.GraphicsDevice);

    /// <summary>
    /// Creates a copy of the texture whose RGB channels are multiplied
    /// by each pixel's alpha value.
    /// </summary>
    /// <param name="texture">The source texture to process.</param>
    /// <returns>A new texture containing premultiplied-alpha pixel data.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D ToPremultiplyAlpha(this Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var data = texture.GetPixels();

        for (int i = 0; i < data.Length; i++)
        {
            var c = data[i];
            float a = c.A / 255f;

            data[i] = new Color(
                (byte)MathF.Round(c.R * a),
                (byte)MathF.Round(c.G * a),
                (byte)MathF.Round(c.B * a),
                c.A);
        }

        var result = new Texture2D(
            texture.GraphicsDevice,
            texture.Width,
            texture.Height,
            false,
            texture.Format);

        result.SetData(data);

        return result;
    }

    /// <summary>
    /// Creates a binary color mask by classifying each source pixel
    /// with the specified predicate.
    /// </summary>
    /// <param name="texture">The source texture to classify.</param>
    /// <param name="matchColor">
    /// The color assigned to pixels for which <paramref name="predicate"/>
    /// returns <see langword="true"/>.
    /// </param>
    /// <param name="backgroundColor">
    /// The color assigned to pixels for which <paramref name="predicate"/>
    /// returns <see langword="false"/>.
    /// </param>
    /// <param name="predicate">The function used to classify each source pixel.</param>
    /// <returns>A new texture containing the generated binary color mask.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="texture"/> or <paramref name="predicate"/>
    /// is <see langword="null"/>.
    /// </exception>
    public static Texture2D CreateBinaryColorMask(
        this Texture2D texture,
        Color matchColor,
        Color backgroundColor,
        Func<Color, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(predicate);

        var data = texture.GetPixels();

        for (int i = 0; i < data.Length; i++)
            data[i] = predicate(data[i]) ? matchColor : backgroundColor;

        var result = new Texture2D(
            texture.GraphicsDevice,
            texture.Width,
            texture.Height,
            false,
            texture.Format);

        result.SetData(data);

        return result;
    }

    /// <summary>
    /// Creates a black-and-white binary color mask by classifying each
    /// source pixel with the specified predicate.
    /// </summary>
    /// <param name="texture">The source texture to classify.</param>
    /// <param name="predicate">The function used to classify each source pixel.</param>
    /// <returns>
    /// A new texture in which matching pixels are white and
    /// non-matching pixels are black.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="texture"/> or <paramref name="predicate"/>
    /// is <see langword="null"/>.
    /// </exception>
    public static Texture2D CreateBinaryColorMask(
        this Texture2D texture,
        Func<Color, bool> predicate)
    {
        return CreateBinaryColorMask(texture, Color.White, Color.Black, predicate);
    }

    /// <summary>
    /// Creates a binary color mask by comparing each source pixel with
    /// the specified match color using an RGB distance threshold.
    /// </summary>
    /// <param name="texture">The source texture to classify.</param>
    /// <param name="matchColor">
    /// The color against which source pixels are compared and the color
    /// assigned to matching pixels.
    /// </param>
    /// <param name="backgroundColor">
    /// The color assigned to pixels outside the specified threshold.
    /// </param>
    /// <param name="threshold">
    /// The maximum RGB distance from <paramref name="matchColor"/> for
    /// a pixel to be considered a match.
    /// </param>
    /// <returns>A new texture containing the generated binary color mask.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D CreateBinaryColorMask(
        this Texture2D texture,
        Color matchColor,
        Color backgroundColor,
        byte threshold)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var data = texture.GetPixels();

        for (int i = 0; i < data.Length; i++)
        {
            var c = data[i];

            int dr = c.R - matchColor.R;
            int dg = c.G - matchColor.G;
            int db = c.B - matchColor.B;

            int distanceSquared = dr * dr + dg * dg + db * db;

            data[i] = distanceSquared <= threshold * threshold
                ? matchColor
                : backgroundColor;
        }

        var result = new Texture2D(
            texture.GraphicsDevice,
            texture.Width,
            texture.Height,
            false,
            texture.Format);

        result.SetData(data);

        return result;
    }
}