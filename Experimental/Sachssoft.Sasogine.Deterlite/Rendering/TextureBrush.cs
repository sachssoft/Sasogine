using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Resources;
namespace Sachssoft.Sasogine.Presentation.Rendering
{
    public class TextureBrush : IBrush
    {
        private float _opacity = 1f;
        private ITextureRegion? _textureRegion;

        public TextureBrush(Texture2D texture) : this(texture, null, 1f) { }

        public TextureBrush(Texture2D texture, ITextureRegion? textureRegion) : this(texture, textureRegion, 1f) { }

        public TextureBrush(Texture2D texture, ITextureRegion? textureRegion, float opacity)
        {
            _opacity = float.Clamp(opacity, 0f, 1f);
            _textureRegion = textureRegion;
        }

        public float Opacity
        {
            get => _opacity;
            set
            {
                var clamped = float.Clamp(value, 0f, 1f);
                if (!float.Abs(_opacity - clamped).Equals(0f))
                    _opacity = clamped;
            }
        }

        public ITextureRegion? TextureRegion
        {
            get => _textureRegion;
            set
            {
                if (!ReferenceEquals(_textureRegion, value))
                    _textureRegion = value;
            }
        }

        public void Render(Bounds bounds, IRenderContext context)
        {
            if (_textureRegion == null) // Klassischer Region
            {

            }
            else if (_textureRegion != null)
            {
                var pos = bounds.Location;

                //_textureRegion.Draw(pos, )
            }
        }
    }
}
