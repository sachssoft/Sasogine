using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Graphics;

public static class Texture2DExtensions
{
    // ---------------------------
    // Core Helpers
    // ---------------------------

    public static Color[] GetPixels(this Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var data = new Color[texture.Width * texture.Height];
        texture.GetData(data);

        return data;
    }

    public static void SetPixels(this Texture2D texture, Color[] data)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length != texture.Width * texture.Height)
            throw new ArgumentException("Pixel array size does not match texture dimensions.");

        texture.SetData(data);
    }

    // ---------------------------
    // Copy / Clone
    // ---------------------------

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

    // ---------------------------
    // Crop
    // ---------------------------

    public static Texture2D Crop(this Texture2D texture, Rectangle sourceRect, GraphicsDevice graphicsDevice)
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

    public static Texture2D Crop(this Texture2D texture, Rectangle sourceRect)
        => Crop(texture, sourceRect, texture.GraphicsDevice);

    // ---------------------------
    // Tile Crop (SpriteSheet)
    // ---------------------------

    public static Texture2D TileCrop(this Texture2D texture, Point cell, PixelSize cellSize, GraphicsDevice graphicsDevice)
    {
        var rect = new Rectangle(
            cell.X * cellSize.Width,
            cell.Y * cellSize.Height,
            cellSize.Width,
            cellSize.Height);

        return Crop(texture, rect, graphicsDevice);
    }

    public static Texture2D TileCrop(this Texture2D texture, Point cell, PixelSize cellSize)
        => TileCrop(texture, cell, cellSize, texture.GraphicsDevice);

    // ---------------------------
    // Premultiply Alpha
    // ---------------------------

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

    // ---------------------------
    // Binary color mask using predicate (pixel classification)
    // ---------------------------

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
        {
            data[i] = predicate(data[i]) ? matchColor : backgroundColor;
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

    public static Texture2D CreateBinaryColorMask(
        this Texture2D texture,
        Func<Color, bool> predicate)
    {
        return CreateBinaryColorMask(
            texture,
            Color.White,
            Color.Black,
            predicate);
    }

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