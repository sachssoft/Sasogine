using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics
{
    public static class Texture2DScaler
    {
        /// <summary>
        /// Halbiert die Auflösung eines Textures mit einfachem Box-Filter.
        /// Jeder neue Pixel ist der Durchschnitt von 2x2 Pixeln.
        /// Ideal für MipMap-Erzeugung.
        /// </summary>
        public static Texture2D DownscaleBox(GraphicsDevice device, Texture2D source)
        {
            int newWidth = source.Width / 2;
            int newHeight = source.Height / 2;

            if (newWidth <= 0 || newHeight <= 0)
                return source;

            Color[] srcData = new Color[source.Width * source.Height];
            source.GetData(srcData);

            Color[] dstData = new Color[newWidth * newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int sx = x * 2;
                    int sy = y * 2;

                    Color c1 = srcData[sy * source.Width + sx];
                    Color c2 = srcData[sy * source.Width + (sx + 1)];
                    Color c3 = srcData[(sy + 1) * source.Width + sx];
                    Color c4 = srcData[(sy + 1) * source.Width + (sx + 1)];

                    dstData[y * newWidth + x] = new Color(
                        (c1.R + c2.R + c3.R + c4.R) / 4,
                        (c1.G + c2.G + c3.G + c4.G) / 4,
                        (c1.B + c2.B + c3.B + c4.B) / 4,
                        (c1.A + c2.A + c3.A + c4.A) / 4
                    );
                }
            }

            Texture2D result = new Texture2D(device, newWidth, newHeight);
            result.SetData(dstData);
            return result;
        }

        /// <summary>
        /// Skaliert ein Texture auf eine beliebige Größe mit Bilinear-Interpolation.
        /// Etwas langsamer, aber bessere Qualität für Thumbnails.
        /// </summary>
        public static Texture2D DownscaleBilinear(GraphicsDevice device, Texture2D source, int targetWidth, int targetHeight)
        {
            Color[] srcData = new Color[source.Width * source.Height];
            source.GetData(srcData);

            Color[] dstData = new Color[targetWidth * targetHeight];

            float xRatio = (float)(source.Width - 1) / targetWidth;
            float yRatio = (float)(source.Height - 1) / targetHeight;

            for (int y = 0; y < targetHeight; y++)
            {
                float sy = y * yRatio;
                int y0 = (int)sy;
                int y1 = int.Min(y0 + 1, source.Height - 1);
                float fy = sy - y0;

                for (int x = 0; x < targetWidth; x++)
                {
                    float sx = x * xRatio;
                    int x0 = (int)sx;
                    int x1 = int.Min(x0 + 1, source.Width - 1);
                    float fx = sx - x0;

                    Color c00 = srcData[y0 * source.Width + x0];
                    Color c10 = srcData[y0 * source.Width + x1];
                    Color c01 = srcData[y1 * source.Width + x0];
                    Color c11 = srcData[y1 * source.Width + x1];

                    byte r = (byte)(
                        c00.R * (1 - fx) * (1 - fy) +
                        c10.R * fx * (1 - fy) +
                        c01.R * (1 - fx) * fy +
                        c11.R * fx * fy);

                    byte g = (byte)(
                        c00.G * (1 - fx) * (1 - fy) +
                        c10.G * fx * (1 - fy) +
                        c01.G * (1 - fx) * fy +
                        c11.G * fx * fy);

                    byte b = (byte)(
                        c00.B * (1 - fx) * (1 - fy) +
                        c10.B * fx * (1 - fy) +
                        c01.B * (1 - fx) * fy +
                        c11.B * fx * fy);

                    byte a = (byte)(
                        c00.A * (1 - fx) * (1 - fy) +
                        c10.A * fx * (1 - fy) +
                        c01.A * (1 - fx) * fy +
                        c11.A * fx * fy);

                    dstData[y * targetWidth + x] = new Color(r, g, b, a);
                }
            }

            Texture2D result = new Texture2D(device, targetWidth, targetHeight);
            result.SetData(dstData);
            return result;
        }
    }
}
