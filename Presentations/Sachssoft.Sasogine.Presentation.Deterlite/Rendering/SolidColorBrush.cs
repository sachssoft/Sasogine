using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Rendering
{
    public class SolidColorBrush : IBrush
    {
        private Color _color;
        private float _opacity = 1f;
        private bool _invalidate = true; // true = Cache muss aktualisiert werden
        private Color _renderColor;

        public SolidColorBrush() : this(Color.Black, 1f) { }

        public SolidColorBrush(Color color) : this(color, 1f) { }

        public SolidColorBrush(Color color, float opacity)
        {
            _color = color;
            _opacity = float.Clamp(opacity, 0f, 1f);
            _invalidate = true;
        }

        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    _invalidate = true;
                }
            }
        }

        public float Opacity
        {
            get => _opacity;
            set
            {
                var clamped = float.Clamp(value, 0f, 1f);
                if (!float.Abs(_opacity - clamped).Equals(0f))
                {
                    _opacity = clamped;
                    _invalidate = true;
                }
            }
        }

        public void Render(Bounds bounds, IRenderContext context)
        {
            if (_invalidate)
            {
                _renderColor = new Color(
                    _color.R,
                    _color.G,
                    _color.B,
                    (byte)(_color.A * _opacity)
                );
                _invalidate = false;
            }

            context.DrawRectangle(bounds, _renderColor);
        }
    }
}