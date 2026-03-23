using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics;

public static class GraphicsDeviceExtension
{
    public static Texture2D CreateEmptyTexture(this GraphicsDevice graphics_device, Color color)
    {
        var tex = new Texture2D(graphics_device, 1, 1);
        tex.SetData<Color>([color]);
        return tex;
    }

    public static Texture2D CreatePremultiplied(this Texture2D texture)
    {
        int w = texture.Width;
        int h = texture.Height;
        Color[] data = new Color[w * h];
        texture.GetData(data);

        for (int i = 0; i < data.Length; i++)
        {
            float alpha = data[i].A / 255f;
            data[i].R = (byte)(data[i].R * alpha);
            data[i].G = (byte)(data[i].G * alpha);
            data[i].B = (byte)(data[i].B * alpha);
        }

        Texture2D result = new Texture2D(texture.GraphicsDevice, w, h);
        result.SetData(data);
        return result;
    }

    //public static Texture2D CreateScreenhot(this GraphicsDevice graphics_device)
    //{
    //    var width = graphics_device.PresentationParameters.BackBufferWidth;
    //    var height = graphics_device.PresentationParameters.BackBufferHeight;

    //    var back_buffer = new int[width * height];
    //    graphics_device.GetBackBufferData(back_buffer);

    //    var texture = new Texture2D(graphics_device, width, height, false, graphics_device.PresentationParameters.BackBufferFormat);
    //    texture.SetData(back_buffer);

    //    return texture;
    //}
}
