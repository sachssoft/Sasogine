using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;
using Sachssoft.Sasogine.Presentation.Deterlite.Styling;
namespace Sachssoft.Sasogine.Presentation.Deterlite.Rendering
{
    public class TextureBrush : IBrush
    {
        private float _opacity = 1f;
        private ITextureRegion? _textureRegion;

        public TextureBrush() : this(null, 1f) { }

        public TextureBrush(ITextureRegion? textureRegion) : this(textureRegion, 1f) { }

        public TextureBrush(ITextureRegion? textureRegion, float opacity)
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
        }

        IStylePart IStylePart.Create(Skin sheet, PropertyMap properties)
        {
            float opacity = 1f;
            ITextureRegion? region = null;

            foreach (var entry in properties)
            {
                switch (entry.Name)
                {
                    case nameof(TextureBrush.TextureRegion):
                        if (entry.Value is StyleBinding binding)
                            region = binding.Resolve(sheet) as ITextureRegion;
                        break;

                    case nameof(TextureBrush.Opacity):
                        if (entry.Value is float o)
                            opacity = o;
                        break;
                }
            }

            return new TextureBrush(region, opacity);
        }
    }
}
