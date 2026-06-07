using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Embedding
{
    public sealed class SharedTextureContext
    {
        private readonly int _textureHandleId = 0;

        public SharedTextureContext(RenderTarget2D renderTarget)
        {
#if MONOGAME_DESKTOPGL
            _textureHandleId = GetTextureId(renderTarget);
#elif MONOGAME_WINDOWSDX
#elif MONOGAME_VULKAN
#endif
        }

        public int TextureHandleId => _textureHandleId;

#if MONOGAME_DESKTOPGL
        private static int GetTextureId(RenderTarget2D rt)
        {
            var texture = rt.GetType()
                .GetProperty("Texture",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(rt) as Texture2D;

            if (texture == null)
                return 0;

            return GetGLTextureId(texture);
        }

        private static int GetGLTextureId(Texture2D texture)
        {
            var field = typeof(Texture2D)
                .GetField("glTexture",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
                throw new NotSupportedException(
                    "glTexture nicht gefunden → nur DesktopGL möglich");

            return Convert.ToInt32(field.GetValue(texture));
        }
#endif
    }
}
