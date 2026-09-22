using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Provides utility methods for creating commonly used two-dimensional textures.
/// </summary>
public static class Texture2DUtils
{
    /// <summary>
    /// Creates a 1x1 texture filled with the specified color.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the texture.</param>
    /// <param name="color">The color used to fill the texture.</param>
    /// <returns>A new 1x1 texture containing the specified color.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D CreateEmptyTexture(GraphicsDevice graphicsDevice, Color color)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData([color]);

        return texture;
    }

    /// <summary>
    /// Creates a texture of the specified size filled with a single color.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the texture.</param>
    /// <param name="width">The width of the texture in pixels.</param>
    /// <param name="height">The height of the texture in pixels.</param>
    /// <param name="color">The color used to fill the texture.</param>
    /// <returns>A new texture filled with the specified color.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="width"/> or <paramref name="height"/>
    /// is less than or equal to zero.
    /// </exception>
    public static Texture2D CreateFilledTexture(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        Color color)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        var texture = new Texture2D(graphicsDevice, width, height);

        var data = new Color[width * height];
        Array.Fill(data, color);

        texture.SetData(data);

        return texture;
    }

    /// <summary>
    /// Creates a checkerboard texture using two alternating colors.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the texture.</param>
    /// <param name="width">The width of the texture in pixels.</param>
    /// <param name="height">The height of the texture in pixels.</param>
    /// <param name="cellSize">The width and height of each checkerboard cell in pixels.</param>
    /// <param name="colorA">The color used for the first set of alternating cells.</param>
    /// <param name="colorB">The color used for the second set of alternating cells.</param>
    /// <returns>A new texture containing the generated checkerboard pattern.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="width"/>, <paramref name="height"/>,
    /// or <paramref name="cellSize"/> is less than or equal to zero.
    /// </exception>
    public static Texture2D CreateCheckerboard(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        int cellSize,
        Color colorA,
        Color colorB)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        if (cellSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(cellSize));

        var texture = new Texture2D(graphicsDevice, width, height);
        var data = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            int cellY = y / cellSize;

            for (int x = 0; x < width; x++)
            {
                int cellX = x / cellSize;
                bool even = ((cellX + cellY) & 1) == 0;

                data[y * width + x] = even
                    ? colorA
                    : colorB;
            }
        }

        texture.SetData(data);

        return texture;
    }
}