using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.UI.Deterlite.Layouts;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Layouts
{
    public class StackLayout : LayoutBase
    {
        private Orientation _orientation = Orientation.Vertical;
        private float _spacing = 0f;

        public StackLayout() : this(Orientation.Vertical)
        {
        }

        public StackLayout(Orientation orientation) : this(orientation, 0f)
        {
        }

        public StackLayout(Orientation orientation, float spacing)
        {
            _orientation = orientation;
            _spacing = spacing;
        }

        // Ausrichtung
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value) return;
                _orientation = value;
                Invalidate();
            }
        }

        // Abstand
        public float Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                Invalidate();
            }
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
    }
}