using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics;

public static class Texture2DUtils
{
    /// <summary>
    /// Creates a 1x1 texture filled with the specified color.
    /// </summary>
    public static Texture2D CreateEmptyTexture(GraphicsDevice graphicsDevice, Color color)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData([color]);

        return texture;
    }

    /// <summary>
    /// Creates a texture of arbitrary size filled with a single color.
    /// </summary>
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
    /// Creates a checkerboard texture useful for debugging.
    /// </summary>
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