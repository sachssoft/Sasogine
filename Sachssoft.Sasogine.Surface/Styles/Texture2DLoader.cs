using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources.Loaders;
using System;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public static class Texture2DLoader
    {
        public static Texture2D Load<TLoader>(string filePath, LoaderOptions options, Func<string, TLoader> loaderInstance)
            where TLoader : LoaderBase
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.GraphicsDevice == null) throw new InvalidOperationException(filePath + ": GraphicsDevice is not set in LoaderOptions.");

            var loader = loaderInstance.Invoke(filePath);
            return Texture2D.FromStream(options.GraphicsDevice, loader.GetStream());
        }
    }
}
