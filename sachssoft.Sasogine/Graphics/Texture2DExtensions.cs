using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using sachssoft.Sasogine.Graphics.Colors;

namespace sachssoft.Sasogine.Graphics;

public static class Texture2DExtensions
{
    public static Color[,] GetPixelColors(this Texture2D texture)
    {
        var colors_1d = new Color[texture.Width * texture.Height];
        var colors_2d = new Color[texture.Width, texture.Height];

        texture.GetData(colors_1d);

        for (int x = 0; x < texture.Width; x++)
        {
            for (int y = 0; y < texture.Height; y++)
            {
                colors_2d[x, y] = colors_1d[x + y * texture.Width];
            }
        }

        return colors_2d;
    }

    public static void SetPixelColors(this Texture2D texture, Color[,] colors)
    {
        var colors_1d = new Color[texture.Width * texture.Height];

        for (int x = 0; x < texture.Width; x++)
        {
            for (int y = 0; y < texture.Height; y++)
            {
                colors_1d[x + y * texture.Width] = colors[x, y];
            }
        }

        texture.SetData(colors_1d);
    }

    //public static Texture2D ConvertToNormal(this RenderTarget2D rt)
    //{

    //    //var colors = rt.GetPixelColors();
    //    //var new_tex = new Texture2D(rt.GraphicsDevice, rt.Width, rt.Height);
    //    //SetPixelColors(new_tex, colors);
    //    //return new_tex;
    //}

    public static Texture2D CreateMask(this Texture2D texture, Color mask_color)
        => CreateMask(texture, mask_color, IMyGameApp.Current.GraphicsDevice);

    public static Texture2D CreateMask(this Texture2D texture, Color mask_color, GraphicsDevice graphics_device)
    {
        var mask_tex = new Texture2D(graphics_device, texture.Width, texture.Height);

        var tex_pixels = texture.GetPixelColors();
        var mask_pixels = new Color[texture.Width, texture.Height];

        for (var y = 0; y < texture.Height; y++)
        {
            for (var x = 0; x < texture.Width; x++)
            {
                mask_pixels[x, y] = (tex_pixels[x, y] == mask_color) ? Color.Transparent : mask_color;
            }
        }

        mask_tex.SetPixelColors(mask_pixels);
        return mask_tex;
    }

    public static Texture2D TileCrop(this Texture2D texture, Point cell, Point size)
        => Crop(texture, new Point(cell.X * size.X, cell.Y * size.Y), size, IMyGameApp.Current.GraphicsDevice);

    public static Texture2D TileCrop(this Texture2D texture, Point cell, Point size, GraphicsDevice graphics_device)
        => Crop(texture, new Point(cell.X * size.X, cell.Y * size.Y), size, graphics_device);

    public static Texture2D Crop(this Texture2D texture, Point position, Point size)
        => Crop(texture, position, size, IMyGameApp.Current.GraphicsDevice);

    public static Texture2D Crop(this Texture2D texture, Point position, Point size, GraphicsDevice graphics_device)
    {
        var crop = new Texture2D(graphics_device, size.X, size.Y);
        var source_rect = new Rectangle(position.X, position.Y, size.X, size.Y);
        var data = new Color[size.X * size.Y];

        // Überträgt ein ausgewählter Kachel zum neuen Textur
        texture.GetData<Color>(0, source_rect, data, 0, data.Length);
        crop.SetData(data);

        return crop;
    }

    public static Texture2D Clone(this Texture2D texture)
        => Clone(texture, IMyGameApp.Current.GraphicsDevice);

    public static Texture2D Clone(this Texture2D texture, GraphicsDevice graphics_device)
    {
        var width = texture.Width;
        var height = texture.Height;

        var new_texture = new Texture2D(graphics_device, width, height);
        var color_data = new Color[width * height];

        texture.GetData(color_data);
        new_texture.SetData(color_data);

        return new_texture;
    }

    public static Texture2D Blend<T>(this Texture2D texture, Color color, float amount)
        where T : IColorBlender, new()
        => Blend<T>(texture, color, amount, IMyGameApp.Current.GraphicsDevice);

    public static Texture2D Blend<T>(this Texture2D texture, Color color, float amount, GraphicsDevice graphics_device)
       where T : IColorBlender, new()
    {
        var blender = new T();
        int width = texture.Width;
        int height = texture.Height;
        var colors = new Color[width * height];

        texture.GetData(colors);

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = blender.Blend(colors[i], color, new ColorRange(amount));
        }

        var new_texture = new Texture2D(graphics_device, width, height);
        new_texture.SetData(colors);
        return new_texture;
    }

    public static Texture2D Adjust<T>(this Texture2D texture, float amount)
        where T : IColorTransformer, new()
        => Adjust<T>(texture, amount, IMyGameApp.Current.GraphicsDevice);

    // Nicht-destruktiv – gibt neue Textur zurück
    public static Texture2D Adjust<T>(this Texture2D texture, float amount, GraphicsDevice graphics_device)
        where T : IColorTransformer, new()
    {
        var transformer = new T();
        int width = texture.Width;
        int height = texture.Height;
        var colors = new Color[width * height];

        texture.GetData(colors);

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = transformer.Transform(colors[i], new ColorRange(amount));
        }

        var new_texture = new Texture2D(graphics_device, width, height);
        new_texture.SetData(colors);
        return new_texture;
    }

    // Destruktiv – verändert Originaltextur direkt
    public static void AdjustInPlace<T>(this Texture2D texture, float amount)
        where T : IColorTransformer, new()
    {
        var transformer = new T();
        int width = texture.Width;
        int height = texture.Height;
        var colors = new Color[width * height];

        texture.GetData(colors);

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = transformer.Transform(colors[i], new ColorRange(amount));
        }

        texture.SetData(colors); // Achtung: ersetzt Originalinhalt!
    }

    public static Texture2D Adjust(this Texture2D texture, ColorTransformerFactory factory)
        => Adjust(texture, factory, IMyGameApp.Current.GraphicsDevice);

    public static Texture2D Adjust(this Texture2D texture, ColorTransformerFactory factory, GraphicsDevice graphics_device)
    {
        var width = texture.Width;
        var height = texture.Height;
        var colors = new Color[width * height];

        texture.GetData(colors);

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = factory.Transform(colors[i]);
        }

        var new_texture = new Texture2D(graphics_device, width, height);
        new_texture.SetData(colors);
        return new_texture;
    }

    public static void AdjustInPlace(this Texture2D texture, ColorTransformerFactory factory)
    {
        var width = texture.Width;
        var height = texture.Height;
        var colors = new Color[width * height];

        texture.GetData(colors);

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = factory.Transform(colors[i]);
        }

        texture.SetData(colors);
    }

}
