using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Sachssoft.Sasogine.Graphics.Utils
{
    public static class Screenshot
    {
        private static Color[]? _buffer;

        // Öffentliche Methoden für Backbuffer und RenderTarget
        public static Texture2D Create(GraphicsDevice graphicsDevice)
        {
            int width = graphicsDevice.PresentationParameters.BackBufferWidth;
            int height = graphicsDevice.PresentationParameters.BackBufferHeight;

            return CreateTexture(graphicsDevice, width, height, data => graphicsDevice.GetBackBufferData(data));
        }

        public static Texture2D FromRenderTarget(GraphicsDevice graphicsDevice, RenderTarget2D target)
        {
            return CreateTexture(graphicsDevice, target.Width, target.Height, data => target.GetData(data));
        }

        // Capture direkt in Stream, Backbuffer oder RenderTarget
        public static void Capture(GraphicsDevice graphicsDevice, Stream stream, ScreenshotImageFormat format = ScreenshotImageFormat.PNG)
        {
            using var texture = Create(graphicsDevice);
            SaveTexture(texture, stream, format);
        }

        public static void Capture(RenderTarget2D target, GraphicsDevice graphicsDevice, Stream stream, ScreenshotImageFormat format = ScreenshotImageFormat.PNG)
        {
            using var texture = FromRenderTarget(graphicsDevice, target);
            SaveTexture(texture, stream, format);
        }

        // Gemeinsame Methode für PNG/JPG
        private static void SaveTexture(Texture2D texture, Stream stream, ScreenshotImageFormat format)
        {
            switch (format)
            {
                case ScreenshotImageFormat.PNG:
                    texture.SaveAsPng(stream, texture.Width, texture.Height);
                    break;
                case ScreenshotImageFormat.JPG:
                    texture.SaveAsJpeg(stream, texture.Width, texture.Height);
                    break;
                default:
                    texture.SaveAsPng(stream, texture.Width, texture.Height);
                    break;
            }
        }

        private static Color[] GetBuffer(int size)
        {
            if (_buffer == null || _buffer.Length != size)
                _buffer = new Color[size];
            return _buffer;
        }

        // Interne generische Methode für Backbuffer oder RenderTarget
        private static Texture2D CreateTexture(GraphicsDevice graphicsDevice, int width, int height, System.Action<Color[]> getData)
        {
            var data = GetBuffer(width * height);
            getData(data);

            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            texture.SetData(data);

            return texture;
        }
    }
}