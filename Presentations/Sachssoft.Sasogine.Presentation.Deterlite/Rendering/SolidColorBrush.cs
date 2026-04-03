using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;
using Sachssoft.Sasogine.Presentation.Deterlite.Styling;

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

        IStylePart IStylePart.Create(Skin sheet, PropertyMap properties)
        {
            Color color = default;
            float opacity = 1f; // Default-Wert, falls nicht gesetzt

            foreach (var entry in properties)
            {
                switch (entry.Name)
                {
                    case nameof(SolidColorBrush.Color):
                        if (entry.Value is Color clr)
                            color = clr;
                        else if (entry.Value is string str)
                            color = ColorUtils.FromHex(str);
                        break;

                    case nameof(SolidColorBrush.Opacity):
                        if (entry.Value is float o)
                            opacity = o;
                        break;
                }
            }

            return new SolidColorBrush(color, opacity);
        }
    }
}